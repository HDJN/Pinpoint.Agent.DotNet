#include "Interceptor.h";
#include "FatILWriter.h";
#include "SmallILWriter.h";
#include "ProfilerLoggers.h"
#include "ConvertedFatILWriter.h";

#define Check(hr) if (FAILED(hr)) exit(1);

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE call;
	BYTE callToken[4];
} GeneralInterceptBeforeIL;

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE call;
	BYTE callToken[4];
} GeneralInterceptAfterIL;

bool Interceptor::IsTiny(LPCBYTE methodBytes)
{
	return ((COR_ILMETHOD_TINY*)methodBytes)->IsTiny();
}

mdModuleRef Interceptor::GetAssemblyToken(IMetaDataEmit *metaDataEmit, WCHAR *assemblyName)
{
	ASSEMBLYMETADATA assemblyMetaData;
	ZeroMemory(&assemblyMetaData, sizeof(assemblyMetaData));
	assemblyMetaData.usMajorVersion = 1;
	assemblyMetaData.usMinorVersion = 0;
	assemblyMetaData.usBuildNumber = 0;
	assemblyMetaData.usRevisionNumber = 0;

	const BYTE keyEMCA[] = { 0xf5, 0xc2, 0x2b, 0x8f, 0xbb, 0x0c, 0x47, 0x97 };
	IMetaDataAssemblyEmit* metaDataAssemblyEmit = NULL;
	mdModuleRef assemblyToken;
	Check(metaDataEmit->QueryInterface(IID_IMetaDataAssemblyEmit, (void**)&metaDataAssemblyEmit));
	Check(metaDataAssemblyEmit->DefineAssemblyRef(keyEMCA, sizeof(keyEMCA), assemblyName, &assemblyMetaData, NULL, 0, 0, &assemblyToken));
	metaDataAssemblyEmit->Release();

	return assemblyToken;
}

Interceptor::Interceptor(ICorProfilerInfo *corProfilerInfo)
{
	this->corProfilerInfo = corProfilerInfo;
}

bool Interceptor::IsMatch(const WCHAR *className, const WCHAR *methodName)
{
	return wcscmp(GetClassName2(), className) == 0 &&
		wcscmp(GetMethodName(), methodName) == 0;
}

void *Interceptor::GetInterceptILCode(FunctionInfo *functionInfo)
{
	LPCBYTE oldMethodBytes;
	ULONG oldMethodSize;

	Check(corProfilerInfo->GetILFunctionBody(functionInfo->GetModuleID(), functionInfo->GetToken(), &oldMethodBytes, &oldMethodSize));

	IMetaDataEmit* metaDataEmit = NULL;
	Check(corProfilerInfo->GetModuleMetaData(functionInfo->GetModuleID(), ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit));

	int newMethodSize = 0, ilCodeSize = 0, interceptAfILSize = 0, retOffset = 0;
	void *ilCode;
	void *interceptAfIL;
	ILWriterBase *ilWriterBase;

	ilCode = GetInterceptorBeforeILCode(metaDataEmit, functionInfo, &ilCodeSize);
	interceptAfIL = GetInterceptorAfterILCode(metaDataEmit, functionInfo, &interceptAfILSize, &retOffset);
	newMethodSize = ilCodeSize + interceptAfILSize;

	if (IsTiny(oldMethodBytes))
	{
		g_debugLogger << " -> Tiny Header" << std::endl;

		if (oldMethodSize > (MAX_TINY_FORMAT_SIZE - newMethodSize))
		{
			g_debugLogger << " -> Convertted to fat header" << std::endl;
			ilWriterBase = new ConvertedFatILWriter(corProfilerInfo, functionInfo, oldMethodBytes, oldMethodSize, newMethodSize);
		}
		else
		{
			ilWriterBase = new SmallILWriter(corProfilerInfo, functionInfo, oldMethodBytes, oldMethodSize, newMethodSize);
		}
	}
	else
	{
		g_debugLogger << " -> Fat Header" << std::endl;
		ilWriterBase = new FatILWriter(corProfilerInfo, functionInfo, oldMethodBytes, oldMethodSize, newMethodSize);
	}

	return ilWriterBase->GetNewILBytes(ilCode, ilCodeSize, interceptAfIL, interceptAfILSize, retOffset);
}

mdFieldDef Interceptor::GetFieldToken(FunctionInfo *functionInfo, WCHAR *fieldName)
{
	ModuleID moduleID;
	mdTypeDef classToken;
	IMetaDataImport* metaDataImport = NULL;
	Check(corProfilerInfo->GetModuleMetaData(functionInfo->GetModuleID(), ofRead | ofWrite, IID_IMetaDataImport, (IUnknown**)&metaDataImport));
	Check(corProfilerInfo->GetClassIDInfo(functionInfo->GetClassID(), &moduleID, &classToken));

	mdFieldDef fieldToken;
	Check(metaDataImport->FindField(classToken, fieldName, NULL , NULL, &fieldToken));
	return fieldToken;
}

std::wstring Interceptor::GetModuleVID(FunctionInfo *functionInfo)
{
	ModuleID moduleID;
	mdTypeDef classToken;
	IMetaDataImport* metaDataImport = NULL;
	Check(corProfilerInfo->GetModuleMetaData(functionInfo->GetModuleID(), ofRead | ofWrite, IID_IMetaDataImport, (IUnknown**)&metaDataImport));

	WCHAR moduleName[1024]; ULONG nameSize; GUID mvid;
	Check(metaDataImport->GetScopeProps(moduleName, 1024, &nameSize, &mvid));

	WCHAR strGUID[48];
	StringFromGUID2(mvid, strGUID, 48);

	return std::wstring(strGUID);
}

void *Interceptor::GetGeneralInterceptBeforeIL(IMetaDataEmit *metaDataEmit, const WCHAR *className, const WCHAR *methodName, int *ilCodeSize)
{
	mdTypeRef classToken;
	Check(metaDataEmit->DefineTypeRefByName(GetAssemblyToken(metaDataEmit, L"Pinpoint.Agent"), L"Pinpoint.Profiler.Bootstrap", &classToken));

	//calling convention, argument count, return type, arg type
	const BYTE signature[] = { IMAGE_CEE_CS_CALLCONV_DEFAULT, 2, ELEMENT_TYPE_VOID,
		ELEMENT_TYPE_STRING, ELEMENT_TYPE_STRING };

	mdMemberRef methodToken;
	Check(metaDataEmit->DefineMemberRef(classToken, L"InterceptMethodBegin", signature, sizeof(signature), &methodToken));

	GeneralInterceptAfterIL *ilCode = new GeneralInterceptAfterIL();
	mdString textToken;

	Check(metaDataEmit->DefineUserString(className, wcslen(className), &textToken));
	ilCode->ldstr1 = 0x72;
	memcpy(ilCode->stringToken1, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(methodName, wcslen(methodName), &textToken));
	ilCode->ldstr2 = 0x72;
	memcpy(ilCode->stringToken2, (void*)&textToken, sizeof(textToken));

	ilCode->call = 0x28;
	memcpy(ilCode->callToken, (void*)&methodToken, sizeof(methodToken));

	*ilCodeSize = sizeof(GeneralInterceptAfterIL);
	return ilCode;
}

void *Interceptor::GetGeneralInterceptAfterIL(IMetaDataEmit *metaDataEmit, const WCHAR *className, const WCHAR *methodName, int *ilCodeSize)
{
	mdTypeRef classToken;
	Check(metaDataEmit->DefineTypeRefByName(GetAssemblyToken(metaDataEmit, L"Pinpoint.Agent"), L"Pinpoint.Profiler.Bootstrap", &classToken));

	//calling convention, argument count, return type, arg type
	const BYTE signature[] = { IMAGE_CEE_CS_CALLCONV_DEFAULT, 2, ELEMENT_TYPE_VOID,
		ELEMENT_TYPE_STRING, ELEMENT_TYPE_STRING };

	mdMemberRef methodToken;
	Check(metaDataEmit->DefineMemberRef(classToken, L"InterceptMethodEnd", signature, sizeof(signature), &methodToken));

	GeneralInterceptAfterIL *ilCode = new GeneralInterceptAfterIL();
	mdString textToken;

	Check(metaDataEmit->DefineUserString(className, wcslen(className), &textToken));
	ilCode->ldstr1 = 0x72;
	memcpy(ilCode->stringToken1, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(methodName, wcslen(methodName), &textToken));
	ilCode->ldstr2 = 0x72;
	memcpy(ilCode->stringToken2, (void*)&textToken, sizeof(textToken));

	ilCode->call = 0x28;
	memcpy(ilCode->callToken, (void*)&methodToken, sizeof(methodToken));

	*ilCodeSize = sizeof(GeneralInterceptAfterIL);
	return ilCode;
}
