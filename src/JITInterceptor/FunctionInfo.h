#pragma once

#include <cor.h>
#include <corprof.h>

#define Check(hr) if (FAILED(hr)) exit(1);

struct FunctionInfo
{
	static bool CreateFunctionInfo(ICorProfilerInfo *profilerInfo, FunctionID functionID, FunctionInfo *functionInfo);
	FunctionInfo();
public:
	FunctionInfo(FunctionID functionID, ClassID classID, ModuleID moduleID, mdToken token, LPWSTR functionName, LPWSTR className, LPWSTR assemblyName);
	~FunctionInfo();

	FunctionID GetFunctionID() { return functionID; }
	ClassID GetClassID() { return classID; }
	ModuleID GetModuleID() { return moduleID; }
	mdToken GetToken() { return token; }
	CorElementType GetReturnType() { return returnType; }
	const WCHAR* GetClassName() { return className; }
	const WCHAR* GetFunctionName() { return functionName; }
	const WCHAR* GetAssembly() { return assemblyName; }
	const WCHAR* GetSignatureText() { return signatureText; }
	bool IsValid() { return this != GetNullObject(); }

	static FunctionInfo* GetNullObject();

private:
	FunctionInfo(FunctionID functionID, ClassID classID, ModuleID moduleID, mdToken token, LPWSTR functionName, LPWSTR className, LPWSTR assemblyName, CorElementType returnType, LPWSTR signatureText);
	static PCCOR_SIGNATURE ParseSignature(IMetaDataImport *pMDImport, PCCOR_SIGNATURE signature, WCHAR* szBuffer);
	FunctionID functionID;
	ClassID classID;
	ModuleID moduleID;
	mdToken token;
	CorElementType returnType;
	WCHAR className[1024];
	WCHAR functionName[1024];
	WCHAR assemblyName[1024];
	WCHAR signatureText[1024];
};