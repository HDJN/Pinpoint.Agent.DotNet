#include "stdafx.h"
#include <string>
#include "JITInterceptorImpl.h"
#include "StringHelper.h"
#include "FunctionInfo.h"
#include "ILWriterBase.h"
#include "ProfilerLoggers.h"
#include "Interceptor.h"
#include "SystemHelper.h"
#include "StringHelper.h"

//#include "ILRewriteConsoleLogger.h"
//#include "ILRewriteDebugLogger.h"

#include <wchar.h>
#include <corhlpr.h>
#include <memory>

#pragma comment(lib, "CorGuids.lib")


CRITICAL_SECTION g_cs_ilRewite;


void JITInterceptorImpl::CheckOk(HRESULT hr)
{
	if (hr != S_OK)
		throw "HRESULT is != S_OK";
}

HRESULT JITInterceptorImpl::SetEventMask()
{
	DWORD eventMask = COR_PRF_MONITOR_NONE;
	eventMask |= COR_PRF_MONITOR_JIT_COMPILATION;
	eventMask |= COR_PRF_DISABLE_INLINING;
	eventMask |= COR_PRF_DISABLE_OPTIMIZATIONS;
	eventMask |= COR_PRF_MONITOR_CLASS_LOADS;
	eventMask |= COR_PRF_MONITOR_CACHE_SEARCHES;
	eventMask |= COR_PRF_USE_PROFILE_IMAGES;

	return m_corProfilerInfo->SetEventMask(eventMask);
}


STDMETHODIMP JITInterceptorImpl::QueryInterface(
	REFIID riid,
	void **ppObj)
{
	LPOLESTR clsid = nullptr;

	HRESULT hr = StringFromCLSID(riid, &clsid);
	if (SUCCEEDED(hr))
	{
		std::wstring clsidString(clsid);
		::CoTaskMemFree(clsid);
	}
	if (riid == IID_IUnknown)
	{
		*ppObj = this;
		AddRef();
		return S_OK;
	}

	if (riid == IID_JITInterceptor)
	{
		*ppObj = static_cast<JITInterceptor*>(this);
		AddRef();
		return S_OK;
	}

	if (riid == IID_ICorProfilerCallback)
	{
		*ppObj = static_cast<ICorProfilerCallback*>(this);
		AddRef();
		return S_OK;
	}

	if (riid == IID_ICorProfilerCallback2)
	{
		*ppObj = static_cast<ICorProfilerCallback2*>(this);
		AddRef();
		return S_OK;
	}

	if (riid == IID_ICorProfilerCallback3)
	{
		*ppObj = dynamic_cast<ICorProfilerCallback3*>(this);
		AddRef();
		return S_OK;
	}

	if (riid == IID_ICorProfilerInfo)
	{
		*ppObj = m_corProfilerInfo;
		return S_OK;
	}

	if (riid == IID_ICorProfilerInfo2)
	{
		*ppObj = m_corProfilerInfo2;
		return S_OK;
	}

	*ppObj = NULL;

	return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE JITInterceptorImpl::AddRef()
{
	return InterlockedIncrement(&m_nRefCount);
}

ULONG STDMETHODCALLTYPE JITInterceptorImpl::Release()
{
	long nRefCount = 0;
	nRefCount = InterlockedDecrement(&m_nRefCount);
	return nRefCount;
}

STDMETHODIMP JITInterceptorImpl::ProfilerAttachComplete(void)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ProfilerDetachSucceeded(void)
{
	return S_OK;
};

STDMETHODIMP JITInterceptorImpl::InitializeForAttach(
	/* [in] */ IUnknown *pCorProfilerInfoUnk,
	/* [in] */ void *pvClientData,
	/* [in] */ UINT cbClientData)
{
	return S_OK;
}

//
// Called when a profiler is attached using environment variables
//
STDMETHODIMP JITInterceptorImpl::Initialize(IUnknown *pICorProfilerInfoUnk)
{
	g_debugLogger << "start" << std::endl;
	// get the ICorProfilerInfo interface
	HRESULT hr = pICorProfilerInfoUnk->QueryInterface(IID_ICorProfilerInfo, (LPVOID*)&m_corProfilerInfo);
	if (FAILED(hr))
	{
		return E_FAIL;
	}
	else
	{

	}

	hr = pICorProfilerInfoUnk->QueryInterface(IID_ICorProfilerInfo2, (LPVOID*)&m_corProfilerInfo2);
	if (FAILED(hr))
	{
		m_corProfilerInfo2 = nullptr;
	}
	else
	{

	}

	// Tell the profiler API which events we want to listen to
	// Some events fail when we attach afterwards

	hr = SetEventMask();
	if (FAILED(hr))
	{

	}
	else
	{

	}

	// We do not need echo to OutputDebugString
	//g_profilerData.m_debugLogger.Echo2OutputDebug(false);

	userFunctionNamespaces = Split(WGetEnvVar(std::wstring(L"PINPOINT_USER_FUNCTION_NAMESPACE")), L",");

	InitializeCriticalSection(&g_cs_ilRewite);
	InitializeCriticalSection(&cs_jitCompilationLock);

	interceptors.insert(interceptors.end(), (Interceptor *)new CallHandlerExecutionStep_Execute(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new SqlCommand_ExecuteNonQuery(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new SqlCommand_ExecuteReader(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new SqlCommand_ExecuteScalar(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new HttpWebRequest_GetResponse(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new MySqlCommand_ExecuteReader(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new RedisNativeClient_SendReceive(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new ModelBase_ModelSend(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new FastDFSClient_DownloadFile(m_corProfilerInfo));
	interceptors.insert(interceptors.end(), (Interceptor *)new FastDFSClient_UploadFile(m_corProfilerInfo));

	generalInterceptor = (Interceptor *)new GeneralIntercept(m_corProfilerInfo);

	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ThreadAssignedToOSThread(ThreadID managedThreadID, DWORD osThreadID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::Shutdown()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::AppDomainCreationStarted(AppDomainID appDomainID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::AppDomainCreationFinished(AppDomainID appDomainID, HRESULT hrStatus)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::AppDomainShutdownStarted(AppDomainID appDomainID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::AppDomainShutdownFinished(AppDomainID appDomainID, HRESULT hrStatus)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::AssemblyLoadStarted(AssemblyID assemblyID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::AssemblyLoadFinished(AssemblyID assemblyID, HRESULT hrStatus)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::AssemblyUnloadStarted(AssemblyID assemblyID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::AssemblyUnloadFinished(AssemblyID assemblyID, HRESULT hrStatus)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ModuleLoadStarted(ModuleID moduleID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ModuleLoadFinished(ModuleID moduleID, HRESULT hrStatus)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ModuleUnloadStarted(ModuleID moduleID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ModuleUnloadFinished(ModuleID moduleID, HRESULT hrStatus)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ModuleAttachedToAssembly(ModuleID moduleID, AssemblyID assemblyID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ClassLoadStarted(ClassID classID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ClassLoadFinished(ClassID classID, HRESULT hrStatus)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ClassUnloadStarted(ClassID classID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ClassUnloadFinished(ClassID classID, HRESULT hrStatus)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::FunctionUnloadStarted(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::JITCompilationStarted(FunctionID functionID, BOOL fIsSafeToBlock)
{
	FunctionInfo functionInfo;
	FunctionInfo::CreateFunctionInfo(m_corProfilerInfo, functionID, &functionInfo);
	//g_debugLogger << functionInfo->GetClassNameW() << "." << functionInfo->GetFunctionName() << std::endl;
	std::list<Interceptor *>::iterator itor = interceptors.begin();
	while (itor != interceptors.end())
	{
		Interceptor *current = *itor++;
		if (current->IsMatch(functionInfo.GetClassNameW(), functionInfo.GetFunctionName()))
		{
			if (wcscmp(L"System.Data.SqlClient.SqlCommand", functionInfo.GetClassNameW()) == 0 &&
				wcscmp(L"ExecuteReader", functionInfo.GetFunctionName()) == 0 &&
				wcscmp(L"System.Data.CommandBehavior, String", functionInfo.GetSignatureText()) != 0)
			{
				return S_OK;
			}

			if (wcscmp(L"MySql.Data.MySqlClient.MySqlCommand", functionInfo.GetClassNameW()) == 0 &&
				wcscmp(L"ExecuteReader", functionInfo.GetFunctionName()) == 0 &&
				wcscmp(L"System.Data.CommandBehavior", functionInfo.GetSignatureText()) != 0)
			{
				return S_OK;
			}

			if (wcscmp(L"RabbitMQ.Client.Impl.ModelBase", functionInfo.GetClassNameW()) == 0 &&
				wcscmp(L"BasicPublish", functionInfo.GetFunctionName()) == 0 &&
				wcscmp(L"String, String, bool, RabbitMQ.Client.IBasicProperties, unsigned int8[]", functionInfo.GetSignatureText()) != 0)
			{
				return S_OK;
			}

			RewriteFunction(&functionInfo, current);

			return S_OK;
		}
	}

	std::list<std::wstring>::iterator cs_itor = userFunctionNamespaces.begin();
	while (cs_itor != userFunctionNamespaces.end())
	{
		std::wstring current = *cs_itor++;
		int pos = std::wstring(functionInfo.GetClassNameW()).find(current);
		if (pos >= 0)
		{
			if (wcscmp(L".ctor", functionInfo.GetFunctionName()) != 0 &&
				wcscmp(L".cctor", functionInfo.GetFunctionName()) != 0)
			{
				pos = std::wstring(functionInfo.GetFunctionName()).find(L"set_");
				if (pos >= 0)
				{
					return S_OK;
				}
				pos = std::wstring(functionInfo.GetFunctionName()).find(L"get_");
				if (pos >= 0)
				{
					return S_OK;
				}

				RewriteFunction(&functionInfo, generalInterceptor);
			}

			break;
		}
	}

	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::JITCompilationFinished(FunctionID functionID, HRESULT hrStatus, BOOL fIsSafeToBlock)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::JITCachedFunctionSearchStarted(FunctionID functionID, BOOL *pbUseCachedFunction)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::JITCachedFunctionSearchFinished(FunctionID functionID, COR_PRF_JIT_CACHE result)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::JITFunctionPitched(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::JITInlining(FunctionID callerID, FunctionID calleeID, BOOL *pfShouldInline)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::UnmanagedToManagedTransition(FunctionID functionID, COR_PRF_TRANSITION_REASON reason)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ManagedToUnmanagedTransition(FunctionID functionID, COR_PRF_TRANSITION_REASON reason)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ThreadCreated(ThreadID threadID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ThreadDestroyed(ThreadID threadID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RemotingClientInvocationStarted()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RemotingClientSendingMessage(GUID *pCookie, BOOL fIsAsync)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RemotingClientReceivingReply(GUID *pCookie, BOOL fIsAsync)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RemotingClientInvocationFinished()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RemotingServerReceivingMessage(GUID *pCookie, BOOL fIsAsync)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RemotingServerInvocationStarted()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RemotingServerInvocationReturned()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RemotingServerSendingReply(GUID *pCookie, BOOL fIsAsync)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RuntimeSuspendFinished()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RuntimeSuspendAborted()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RuntimeResumeStarted()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RuntimeResumeFinished()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RuntimeThreadSuspended(ThreadID threadID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RuntimeThreadResumed(ThreadID threadID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::MovedReferences(ULONG cmovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ObjectAllocated(ObjectID objectID, ClassID classID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ObjectsAllocatedByClass(ULONG classCount, ClassID classIDs[], ULONG objects[])
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ObjectReferences(ObjectID objectID, ClassID classID, ULONG objectRefs, ObjectID objectRefIDs[])
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RootReferences(ULONG rootRefs, ObjectID rootRefIDs[])
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionThrown(ObjectID thrownObjectID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionUnwindFunctionEnter(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionUnwindFunctionLeave()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionSearchFunctionEnter(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionSearchFunctionLeave()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionSearchFilterEnter(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionSearchFilterLeave()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionSearchCatcherFound(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionCLRCatcherFound()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionCLRCatcherExecute()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionOSHandlerEnter(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionOSHandlerLeave(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionUnwindFinallyEnter(FunctionID functionID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionUnwindFinallyLeave()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionCatcherEnter(FunctionID functionID,
	ObjectID objectID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ExceptionCatcherLeave()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::COMClassicVTableCreated(ClassID wrappedClassID, REFGUID implementedIID, void *pVTable, ULONG cSlots)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::COMClassicVTableDestroyed(ClassID wrappedClassID, REFGUID implementedIID, void *pVTable)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::ThreadNameChanged(ThreadID threadID, ULONG cchName, WCHAR name[])
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::GarbageCollectionStarted(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::GarbageCollectionFinished()
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::RootReferences2(ULONG cRootRefs, ObjectID rootRefIDs[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIDs[])
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::HandleCreated(GCHandleID handleID, ObjectID initialObjectID)
{
	return S_OK;
}

STDMETHODIMP JITInterceptorImpl::HandleDestroyed(GCHandleID handleID)
{
	return S_OK;
}

void JITInterceptorImpl::RewriteFunction(FunctionInfo *functionInfo, Interceptor *interceptor)
{
	std::wstring fullName = std::wstring(functionInfo->GetClassNameW()) + std::wstring(L".") +
		std::wstring(functionInfo->GetFunctionName());

	EnterCriticalSection(&cs_jitCompilationLock);

	std::list<std::wstring>::iterator methodiItor = interceptedMethods.begin();
	bool exsits = false;
	while (methodiItor != interceptedMethods.end())
	{
		std::wstring method = *methodiItor++;
		if (fullName == method)
		{
			exsits = true;
			break;
		}
	}

	if (!exsits)
	{
		CheckOk(m_corProfilerInfo->SetILFunctionBody(functionInfo->GetModuleID(), functionInfo->GetToken(),
			(LPCBYTE)interceptor->GetInterceptILCode(functionInfo)));
		interceptedMethods.insert(interceptedMethods.end(), fullName);
		g_debugLogger << "intercept method " << functionInfo->GetClassNameW() << "." << functionInfo->GetFunctionName() << std::endl;
	}

	LeaveCriticalSection(&cs_jitCompilationLock);
}

