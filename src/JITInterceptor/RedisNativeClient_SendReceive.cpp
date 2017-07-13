#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE ldarg;
	BYTE call;
	BYTE callToken[4];
} BeforeIL;

typedef struct {
	BYTE callOp;
	BYTE callToken[4];
} AfterIL;

RedisNativeClient_SendReceive::RedisNativeClient_SendReceive(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *RedisNativeClient_SendReceive::GetClassName2()
{
	return L"ServiceStack.Redis.RedisNativeClient";
}

WCHAR *RedisNativeClient_SendReceive::GetMethodName()
{
	return L"SendReceive";
}

void* RedisNativeClient_SendReceive::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, int *ilCodeSize)
{
	mdTypeRef classToken;
	Check(metaDataEmit->DefineTypeRefByName(GetAssemblyToken(metaDataEmit, L"Pinpoint.Agent"), L"Pinpoint.Profiler.Bootstrap", &classToken));

	//calling convention, argument count, return type, arg type
	const BYTE signature[] = { IMAGE_CEE_CS_CALLCONV_DEFAULT, 3, ELEMENT_TYPE_VOID,
		ELEMENT_TYPE_STRING, ELEMENT_TYPE_STRING, ELEMENT_TYPE_OBJECT };

	mdMemberRef methodToken;
	Check(metaDataEmit->DefineMemberRef(classToken, L"InterceptMethodBegin", signature, sizeof(signature), &methodToken));

	BeforeIL *ilCode = new BeforeIL();
	mdString textToken;

	Check(metaDataEmit->DefineUserString(L"ServiceStack.Redis.RedisNativeClient", wcslen(L"ServiceStack.Redis.RedisNativeClient"), &textToken));
	ilCode->ldstr1 = 0x72;
	memcpy(ilCode->stringToken1, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(L"SendReceive", wcslen(L"SendReceive"), &textToken));
	ilCode->ldstr2 = 0x72;
	memcpy(ilCode->stringToken2, (void*)&textToken, sizeof(textToken));

	ilCode->ldarg = 0x03;

	ilCode->call = 0x28;
	memcpy(ilCode->callToken, (void*)&methodToken, sizeof(methodToken));

	*ilCodeSize = sizeof(BeforeIL);
	return ilCode;
}

void *RedisNativeClient_SendReceive::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, int *ilCodeSize, int *offset)
{
	*offset = 1;
	return GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCodeSize);
}