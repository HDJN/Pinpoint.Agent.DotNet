#include "Interceptor.h";

#define Check(hr) if (FAILED(hr)) exit(1);

CallHandlerExecutionStep_Execute::CallHandlerExecutionStep_Execute(ICorProfilerInfo *corProfilerInfo)
	:Interceptor(corProfilerInfo)
{

}

WCHAR *CallHandlerExecutionStep_Execute::GetClassName2()
{
	return L"CallHandlerExecutionStep";
}

WCHAR *CallHandlerExecutionStep_Execute::GetMethodName()
{
	return L"System.Web.HttpApplication.IExecutionStep.Execute";
}

void* CallHandlerExecutionStep_Execute::GetInterceptorBeforeILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, int *ilCodeSize)
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

	Check(metaDataEmit->DefineUserString(L"CallHandlerExecutionStep", wcslen(L"CallHandlerExecutionStep"), &textToken));
	ilCode->ldstr1 = 0x72;
	memcpy(ilCode->stringToken1, (void*)&textToken, sizeof(textToken));

	Check(metaDataEmit->DefineUserString(L"Execute", wcslen(L"Execute"), &textToken));
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

void *CallHandlerExecutionStep_Execute::GetInterceptorAfterILCode(IMetaDataEmit *metaDataEmit, FunctionInfo *functionInfo, int *ilCodeSize, int *offset)
{
	*offset = 1;
	return GetGeneralInterceptAfterIL(metaDataEmit, functionInfo->GetClassNameW(), L"Execute", ilCodeSize);
}