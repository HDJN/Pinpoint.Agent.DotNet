#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE call;
	BYTE callToken[4];
} BeforeIL;

ModelBase_ModelSend::ModelBase_ModelSend(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *ModelBase_ModelSend::GetClassName2()
{
	return L"RabbitMQ.Client.Impl.ModelBase";
}

WCHAR *ModelBase_ModelSend::GetMethodName()
{
	return L"BasicPublish";
}

void* ModelBase_ModelSend::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, int *ilCodeSize)
{
	mdTypeRef classToken;
	Check(metaDataEmit->DefineTypeRefByName(GetAssemblyToken(metaDataEmit, L"Pinpoint.Agent"), L"Pinpoint.Profiler.Bootstrap", &classToken));

	//calling convention, argument count, return type, arg type
	const BYTE signature[] = { IMAGE_CEE_CS_CALLCONV_DEFAULT, 2, ELEMENT_TYPE_VOID,
		ELEMENT_TYPE_STRING, ELEMENT_TYPE_STRING };

	mdMemberRef methodToken;
	Check(metaDataEmit->DefineMemberRef(classToken, L"InterceptMethodBegin", signature, sizeof(signature), &methodToken));

	BeforeIL *ilCode = new BeforeIL();
	mdString textToken;

	Check(metaDataEmit->DefineUserString(GetClassName2(), wcslen(GetClassName2()), &textToken));
	ilCode->ldstr1 = 0x72;
	memcpy(ilCode->stringToken1, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(GetMethodName(), wcslen(GetMethodName()), &textToken));
	ilCode->ldstr2 = 0x72;
	memcpy(ilCode->stringToken2, (void*)&textToken, sizeof(textToken));

	ilCode->call = 0x28;
	memcpy(ilCode->callToken, (void*)&methodToken, sizeof(methodToken));

	*ilCodeSize = sizeof(BeforeIL);
	return ilCode;
}

void *ModelBase_ModelSend::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, int *ilCodeSize, int *offset)
{
	*offset = 1;
	return GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), functionInfo->GetFunctionName(), ilCodeSize);
}