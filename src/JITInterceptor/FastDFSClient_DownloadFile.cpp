#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

FastDFSClient_DownloadFile::FastDFSClient_DownloadFile(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *FastDFSClient_DownloadFile::GetClassName2()
{
	return L"FastDFS.Client.FastDFSClient";
}

WCHAR *FastDFSClient_DownloadFile::GetMethodName()
{
	return L"DownloadFile";
}

void* FastDFSClient_DownloadFile::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, int *ilCodeSize)
{
	mdTypeRef classToken;
	Check(metaDataEmit->DefineTypeRefByName(GetAssemblyToken(metaDataEmit, L"Pinpoint.Agent"), L"Pinpoint.Profiler.Bootstrap", &classToken));

	//calling convention, argument count, return type, arg type
	const BYTE signature[] = { IMAGE_CEE_CS_CALLCONV_DEFAULT, 3, ELEMENT_TYPE_VOID,
		ELEMENT_TYPE_STRING, ELEMENT_TYPE_STRING, ELEMENT_TYPE_OBJECT };

	mdMemberRef methodToken;
	Check(metaDataEmit->DefineMemberRef(classToken, L"InterceptMethodBegin", signature, sizeof(signature), &methodToken));

	InterceptMethodBeginIL1 *ilCode = new InterceptMethodBeginIL1();
	mdString textToken;

	Check(metaDataEmit->DefineUserString(functionInfo->GetClassNameW(), wcslen(functionInfo->GetClassNameW()), &textToken));
	ilCode->ldstr1 = 0x72;
	memcpy(ilCode->stringToken1, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(functionInfo->GetFunctionName(), wcslen(functionInfo->GetFunctionName()), &textToken));
	ilCode->ldstr2 = 0x72;
	memcpy(ilCode->stringToken2, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(L"", wcslen(L""), &textToken));
	ilCode->ldstr3 = 0x72;
	memcpy(ilCode->stringToken3, (void*)&textToken, sizeof(textToken));

	ilCode->call = 0x28;
	memcpy(ilCode->callToken, (void*)&methodToken, sizeof(methodToken));

	*ilCodeSize = sizeof(InterceptMethodBeginIL1);
	return ilCode;
}

void *FastDFSClient_DownloadFile::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, int *ilCodeSize, int *offset)
{
	*offset = 1;
	return GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCodeSize);
}