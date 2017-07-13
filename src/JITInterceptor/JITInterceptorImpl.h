#pragma once

#include <list>
#include <string>
#include <iostream>

#include <cor.h>
#include <corprof.h>
#include <Windows.h>

#include "JITInterceptor_h.h"
#include "Interceptor.h"

class JITInterceptorImpl : 
   public ICorProfilerCallback3,
   public JITInterceptor
{
public :
   JITInterceptorImpl() {};
   ~JITInterceptorImpl() {};

   STDMETHOD(QueryInterface)(REFIID riid, void **ppObj);
   ULONG STDMETHODCALLTYPE AddRef();
   ULONG STDMETHODCALLTYPE Release();
   STDMETHOD(Initialize)(IUnknown *pICorProfilerInfoUnk);
   STDMETHOD(Shutdown)();
   STDMETHOD(AppDomainCreationStarted)(AppDomainID appDomainID);
   STDMETHOD(AppDomainCreationFinished)(AppDomainID appDomainID, HRESULT hrStatus);
   STDMETHOD(AppDomainShutdownStarted)(AppDomainID appDomainID);
   STDMETHOD(AppDomainShutdownFinished)(AppDomainID appDomainID, HRESULT hrStatus);
   STDMETHOD(AssemblyLoadStarted)(AssemblyID assemblyID);
   STDMETHOD(AssemblyLoadFinished)(AssemblyID assemblyID, HRESULT hrStatus);
   STDMETHOD(AssemblyUnloadStarted)(AssemblyID assemblyID);
   STDMETHOD(AssemblyUnloadFinished)(AssemblyID assemblyID, HRESULT hrStatus);
   STDMETHOD(ModuleLoadStarted)(ModuleID moduleID);
   STDMETHOD(ModuleLoadFinished)(ModuleID moduleID, HRESULT hrStatus);
   STDMETHOD(ModuleUnloadStarted)(ModuleID moduleID);
   STDMETHOD(ModuleUnloadFinished)(ModuleID moduleID, HRESULT hrStatus);
   STDMETHOD(ModuleAttachedToAssembly)(ModuleID moduleID, AssemblyID assemblyID);
   STDMETHOD(ClassLoadStarted)(ClassID classID);
   STDMETHOD(ClassLoadFinished)(ClassID classID, HRESULT hrStatus);
   STDMETHOD(ClassUnloadStarted)(ClassID classID);
   STDMETHOD(ClassUnloadFinished)(ClassID classID, HRESULT hrStatus);
   STDMETHOD(FunctionUnloadStarted)(FunctionID functionID);
   STDMETHOD(JITCompilationStarted)(FunctionID functionID, BOOL fIsSafeToBlock);
   STDMETHOD(JITCompilationFinished)(FunctionID functionID, HRESULT hrStatus, BOOL fIsSafeToBlock);
   STDMETHOD(JITCachedFunctionSearchStarted)(FunctionID functionID, BOOL *pbUseCachedFunction);
   STDMETHOD(JITCachedFunctionSearchFinished)(FunctionID functionID, COR_PRF_JIT_CACHE result);
   STDMETHOD(JITFunctionPitched)(FunctionID functionID);
   STDMETHOD(JITInlining)(FunctionID callerID, FunctionID calleeID, BOOL *pfShouldInline);
   STDMETHOD(ThreadCreated)(ThreadID threadID);
   STDMETHOD(ThreadDestroyed)(ThreadID threadID);
   STDMETHOD(ThreadAssignedToOSThread)(ThreadID managedThreadID, DWORD osThreadID);
   STDMETHOD(RemotingClientInvocationStarted)();
   STDMETHOD(RemotingClientSendingMessage)(GUID *pCookie, BOOL fIsAsync);
   STDMETHOD(RemotingClientReceivingReply)(GUID *pCookie, BOOL fIsAsync);
   STDMETHOD(RemotingClientInvocationFinished)();
   STDMETHOD(RemotingServerReceivingMessage)(GUID *pCookie, BOOL fIsAsync);
   STDMETHOD(RemotingServerInvocationStarted)();
   STDMETHOD(RemotingServerInvocationReturned)();
   STDMETHOD(RemotingServerSendingReply)(GUID *pCookie, BOOL fIsAsync);
   STDMETHOD(UnmanagedToManagedTransition)(FunctionID functionID, COR_PRF_TRANSITION_REASON reason);
   STDMETHOD(ManagedToUnmanagedTransition)(FunctionID functionID, COR_PRF_TRANSITION_REASON reason);
   STDMETHOD(RuntimeSuspendStarted)(COR_PRF_SUSPEND_REASON suspendReason);
   STDMETHOD(RuntimeSuspendFinished)();
   STDMETHOD(RuntimeSuspendAborted)();
   STDMETHOD(RuntimeResumeStarted)();
   STDMETHOD(RuntimeResumeFinished)();
   STDMETHOD(RuntimeThreadSuspended)(ThreadID threadid);
   STDMETHOD(RuntimeThreadResumed)(ThreadID threadid);
   STDMETHOD(MovedReferences)(ULONG cmovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[]);
   STDMETHOD(ObjectAllocated)(ObjectID objectID, ClassID classID);
   STDMETHOD(ObjectsAllocatedByClass)(ULONG classCount, ClassID classIDs[], ULONG objects[]);
   STDMETHOD(ObjectReferences)(ObjectID objectID, ClassID classID, ULONG cObjectRefs, ObjectID objectRefIDs[]);
   STDMETHOD(RootReferences)(ULONG cRootRefs, ObjectID rootRefIDs[]);
   STDMETHOD(ExceptionThrown)(ObjectID thrownObjectID);
   STDMETHOD(ExceptionSearchFunctionEnter)(FunctionID functionID);
   STDMETHOD(ExceptionSearchFunctionLeave)();
   STDMETHOD(ExceptionSearchFilterEnter)(FunctionID functionID);
   STDMETHOD(ExceptionSearchFilterLeave)();
   STDMETHOD(ExceptionSearchCatcherFound)(FunctionID functionID);
   STDMETHOD(ExceptionCLRCatcherFound)();
   STDMETHOD(ExceptionCLRCatcherExecute)();
   STDMETHOD(ExceptionOSHandlerEnter)(FunctionID functionID);
   STDMETHOD(ExceptionOSHandlerLeave)(FunctionID functionID);
   STDMETHOD(ExceptionUnwindFunctionEnter)(FunctionID functionID);
   STDMETHOD(ExceptionUnwindFunctionLeave)();
   STDMETHOD(ExceptionUnwindFinallyEnter)(FunctionID functionID);
   STDMETHOD(ExceptionUnwindFinallyLeave)();
   STDMETHOD(ExceptionCatcherEnter)(FunctionID functionID, ObjectID objectID);
   STDMETHOD(ExceptionCatcherLeave)();
   STDMETHOD(COMClassicVTableCreated)(ClassID wrappedClassID, REFGUID implementedIID, void *pVTable, ULONG cSlots);
   STDMETHOD(COMClassicVTableDestroyed)(ClassID wrappedClassID, REFGUID implementedIID, void *pVTable);
   STDMETHOD(ThreadNameChanged)(ThreadID threadId, ULONG cchName, WCHAR name[]);
   STDMETHOD(GarbageCollectionStarted)(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason);
   STDMETHOD(SurvivingReferences)(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], ULONG cObjectIDRangeLength[]);
   STDMETHOD(GarbageCollectionFinished)();
   STDMETHOD(FinalizeableObjectQueued)(DWORD finalizerFlags, ObjectID objectID);
   STDMETHOD(RootReferences2)(ULONG cRootRefs, ObjectID rootRefIds[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIds[]);
   STDMETHOD(HandleCreated)(GCHandleID handleId, ObjectID initialObjectId);
   STDMETHOD(HandleDestroyed)(GCHandleID handleId);
   STDMETHOD(InitializeForAttach)(IUnknown *pCorProfilerInfoUnk, void *pvClientData, UINT cbClientData);
   STDMETHOD(ProfilerAttachComplete)(void);        
   STDMETHOD(ProfilerDetachSucceeded)(void);


   HRESULT SetEventMask(void);
   void CheckOk(HRESULT hr);

private:
   ICorProfilerInfo* m_corProfilerInfo;
   ICorProfilerInfo2* m_corProfilerInfo2;
   std::list<Interceptor *> interceptors;
   Interceptor *generalInterceptor;
   std::list<std::wstring> interceptedMethods;
   CRITICAL_SECTION cs_jitCompilationLock;
   std::list<std::wstring> userFunctionNamespaces;

   long m_nRefCount;   //for managing the reference count
   void RewriteFunction(FunctionInfo *functionInfo, Interceptor *interceptor);
};