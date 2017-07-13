#include "stdafx.h"

#include <cor.h>
#include <corprof.h>
#include <string.h>

#include "FunctionInfo.h"

#define MAX_LENGTH 1024

bool FunctionInfo::CreateFunctionInfo(ICorProfilerInfo *profilerInfo, FunctionID functionID, FunctionInfo *functionInfo)
{
	ClassID classID = 0;
	ModuleID moduleID = 0;
	mdToken tkMethod = 0;
	Check(profilerInfo->GetFunctionInfo(functionID, &classID, &moduleID, &tkMethod));

	WCHAR moduleName[MAX_LENGTH];
	AssemblyID assemblyID;
	Check(profilerInfo->GetModuleInfo(moduleID, NULL, MAX_LENGTH, 0, moduleName, &assemblyID));

	WCHAR assemblyName[MAX_LENGTH];
	Check(profilerInfo->GetAssemblyInfo(assemblyID, MAX_LENGTH, 0, assemblyName, NULL, NULL));

	if (wcscmp(assemblyName, L"Pinpoint.Profiler") == 0)
	{
		return false;
	}

	IMetaDataImport* metaDataImport = NULL;
	mdToken token = NULL;
	Check(profilerInfo->GetTokenAndMetaDataFromFunction(functionID, IID_IMetaDataImport, (LPUNKNOWN *)&metaDataImport, &token));

	mdTypeDef classTypeDef;
	WCHAR functionName[MAX_LENGTH];
	WCHAR className[MAX_LENGTH];
	PCCOR_SIGNATURE signatureBlob;
	ULONG signatureBlobLength;
	DWORD methodAttributes = 0;
	Check(metaDataImport->GetMethodProps(token, &classTypeDef, functionName, MAX_LENGTH, 0, &methodAttributes, &signatureBlob, &signatureBlobLength, NULL, NULL));
	Check(metaDataImport->GetTypeDefProps(classTypeDef, className, MAX_LENGTH, 0, NULL, NULL));
	metaDataImport->Release();

	ULONG callConvension = IMAGE_CEE_CS_CALLCONV_MAX;
	signatureBlob += CorSigUncompressData(signatureBlob, &callConvension);

	ULONG argumentCount;
	signatureBlob += CorSigUncompressData(signatureBlob, &argumentCount);

	LPWSTR returnType = new WCHAR[MAX_LENGTH];
	returnType[0] = '\0';
	CorElementType returnSignature = (CorElementType)*signatureBlob;
	signatureBlob = ParseSignature(metaDataImport, signatureBlob, returnType);

	WCHAR signatureText[MAX_LENGTH] = L"";
	if (wcscmp(functionName, L"System.Web.HttpApplication.IExecutionStep.Execute") == 0 ||
		wcscmp(functionName, L"ExecuteNonQuery") == 0 ||
		wcscmp(functionName, L"ExecuteReader") == 0 ||
		wcscmp(functionName, L"BasicPublish") == 0)
	{
		for (ULONG i = 0; (signatureBlob != NULL) && (i < argumentCount); ++i)
		{
			LPWSTR parameters = new WCHAR[MAX_LENGTH];
			parameters[0] = '\0';
			signatureBlob = ParseSignature(metaDataImport, signatureBlob, parameters);

			if (signatureBlob != NULL)
			{
				if (i > 0)
				{
					wcscat(signatureText, L", ");
				}
				wcscat(signatureText, parameters);
			}
		}
	}

	functionInfo->functionID = functionID;
	functionInfo->classID = classID;
	functionInfo->moduleID = moduleID;
	functionInfo->token = tkMethod;
	wcscpy_s(functionInfo->functionName, functionName);
	wcscpy_s(functionInfo->className, className);
	wcscpy_s(functionInfo->assemblyName, assemblyName);
	wcscpy_s(functionInfo->signatureText, signatureText);
	return true;
}

PCCOR_SIGNATURE FunctionInfo::ParseSignature(IMetaDataImport *metaDataImport, PCCOR_SIGNATURE signature, LPWSTR signatureText)
{
	COR_SIGNATURE corSignature = *signature++;

	switch (corSignature)
	{
	case ELEMENT_TYPE_VOID:
		wcscat(signatureText, L"void");
		break;
	case ELEMENT_TYPE_BOOLEAN:
		wcscat(signatureText, L"bool");
		break;
	case ELEMENT_TYPE_CHAR:
		wcscat(signatureText, L"wchar");
		break;
	case ELEMENT_TYPE_I1:
		wcscat(signatureText, L"int8");
		break;
	case ELEMENT_TYPE_U1:
		wcscat(signatureText, L"unsigned int8");
		break;
	case ELEMENT_TYPE_I2:
		wcscat(signatureText, L"int16");
		break;
	case ELEMENT_TYPE_U2:
		wcscat(signatureText, L"unsigned int16");
		break;
	case ELEMENT_TYPE_I4:
		wcscat(signatureText, L"int32");
		break;
	case ELEMENT_TYPE_U4:
		wcscat(signatureText, L"unsigned int32");
		break;
	case ELEMENT_TYPE_I8:
		wcscat(signatureText, L"int64");
		break;
	case ELEMENT_TYPE_U8:
		wcscat(signatureText, L"unsigned int64");
		break;
	case ELEMENT_TYPE_R4:
		wcscat(signatureText, L"float32");
		break;
	case ELEMENT_TYPE_R8:
		wcscat(signatureText, L"float64");
		break;
	case ELEMENT_TYPE_STRING:
		wcscat(signatureText, L"String");
		break;
	case ELEMENT_TYPE_VAR:
		wcscat(signatureText, L"class variable(unsigned int8)");
		break;
	case ELEMENT_TYPE_MVAR:
		wcscat(signatureText, L"method variable(unsigned int8)");
		break;
	case ELEMENT_TYPE_TYPEDBYREF:
		wcscat(signatureText, L"refany");
		break;
	case ELEMENT_TYPE_I:
		wcscat(signatureText, L"int");
		break;
	case ELEMENT_TYPE_U:
		wcscat(signatureText, L"unsigned int");
		break;
	case ELEMENT_TYPE_OBJECT:
		wcscat(signatureText, L"Object");
		break;
	case ELEMENT_TYPE_SZARRAY:
		signature = ParseSignature(metaDataImport, signature, signatureText);
		wcscat(signatureText, L"[]");
		break;
	case ELEMENT_TYPE_PINNED:
		signature = ParseSignature(metaDataImport, signature, signatureText);
		wcscat(signatureText, L"pinned");
		break;
	case ELEMENT_TYPE_PTR:
		signature = ParseSignature(metaDataImport, signature, signatureText);
		wcscat(signatureText, L"*");
		break;
	case ELEMENT_TYPE_BYREF:
		signature = ParseSignature(metaDataImport, signature, signatureText);
		wcscat(signatureText, L"&");
		break;
	case ELEMENT_TYPE_VALUETYPE:
	case ELEMENT_TYPE_CLASS:
	case ELEMENT_TYPE_CMOD_REQD:
	case ELEMENT_TYPE_CMOD_OPT:
	{
								  mdToken	token;
								  signature += CorSigUncompressToken(signature, &token);

								  WCHAR className[MAX_LENGTH];
								  if (TypeFromToken(token) == mdtTypeRef)
								  {
									  Check(metaDataImport->GetTypeRefProps(token, NULL, className, MAX_LENGTH, NULL));
								  }
								  else
								  {
									  Check(metaDataImport->GetTypeDefProps(token, className, MAX_LENGTH, NULL, NULL, NULL));
								  }

								  wcscat(signatureText, className);
	}
		break;
	case ELEMENT_TYPE_GENERICINST:
	{
									 signature = ParseSignature(metaDataImport, signature, signatureText);

									 wcscat(signatureText, L"<");
									 ULONG arguments = CorSigUncompressData(signature);
									 for (ULONG i = 0; i < arguments; ++i)
									 {
										 if (i != 0)
										 {
											 wcscat(signatureText, L", ");
										 }

										 signature = ParseSignature(metaDataImport, signature, signatureText);
									 }
									 wcscat(signatureText, L">");
	}
		break;
	case ELEMENT_TYPE_ARRAY:
	{
							   signature = ParseSignature(metaDataImport, signature, signatureText);
							   ULONG rank = CorSigUncompressData(signature);
							   if (rank == 0)
							   {
								   wcscat(signatureText, L"[?]");
							   }
							   else
							   {
								   ULONG arraysize = (sizeof(ULONG)* 2 * rank);
								   ULONG *lower = (ULONG *)_alloca(arraysize);
								   memset(lower, 0, arraysize);
								   ULONG *sizes = &lower[rank];

								   ULONG numsizes = CorSigUncompressData(signature);
								   for (ULONG i = 0; i < numsizes && i < rank; i++)
								   {
									   sizes[i] = CorSigUncompressData(signature);
								   }

								   ULONG numlower = CorSigUncompressData(signature);
								   for (ULONG i = 0; i < numlower && i < rank; i++)
								   {
									   lower[i] = CorSigUncompressData(signature);
								   }

								   wcscat(signatureText, L"[");
								   for (ULONG i = 0; i < rank; ++i)
								   {
									   if (i > 0)
									   {
										   wcscat(signatureText, L",");
									   }

									   if (lower[i] == 0)
									   {
										   if (sizes[i] != 0)
										   {
											   WCHAR *size = new WCHAR[MAX_LENGTH];
											   size[0] = '\0';
											   wsprintf(size, L"%d", sizes[i]);
											   wcscat(signatureText, size);
										   }
									   }
									   else
									   {
										   WCHAR *low = new WCHAR[MAX_LENGTH];
										   low[0] = '\0';
										   wsprintf(low, L"%d", lower[i]);
										   wcscat(signatureText, low);
										   wcscat(signatureText, L"...");

										   if (sizes[i] != 0)
										   {
											   WCHAR *size = new WCHAR[MAX_LENGTH];
											   size[0] = '\0';
											   wsprintf(size, L"%d", (lower[i] + sizes[i] + 1));
											   wcscat(signatureText, size);
										   }
									   }
								   }
								   wcscat(signatureText, L"]");
							   }
	}
		break;
	default:
	case ELEMENT_TYPE_END:
	case ELEMENT_TYPE_SENTINEL:
		WCHAR *elementType = new WCHAR[MAX_LENGTH];
		elementType[0] = '\0';
		wsprintf(elementType, L"<UNKNOWN:0x%X>", corSignature);
		wcscat(signatureText, elementType);
		break;
	}

	return signature;
}

FunctionInfo::FunctionInfo()
{
   functionID = 0;
   classID = 0;
   moduleID = 0;
   token = 0;
   className[0] = L'\0';
   functionName[0] = L'\0';
   assemblyName[0] = L'\0';
   signatureText[0] = L'\0';
}

FunctionInfo::FunctionInfo(FunctionID functionID, ClassID classID, ModuleID moduleID, mdToken token, LPWSTR functionName, LPWSTR className, LPWSTR assemblyName)
{
   this->functionID = functionID;
   this->classID = classID;
   this->moduleID = moduleID;
   this->token = token;
   wcscpy_s(this->functionName, functionName);
   wcscpy_s(this->className, className);
   wcscpy_s(this->assemblyName, assemblyName);
}

FunctionInfo::FunctionInfo(FunctionID functionID, ClassID classID, ModuleID moduleID, mdToken token, LPWSTR functionName, LPWSTR className, LPWSTR assemblyName, CorElementType returnType, LPWSTR signatureText)
{
	FunctionInfo::functionID = functionID;
	FunctionInfo::classID = classID;
	FunctionInfo::moduleID = moduleID;
	FunctionInfo::token = token;
	this->returnType = returnType;
	wcscpy_s(this->functionName, functionName);
	wcscpy_s(this->className, className);
	wcscpy_s(this->assemblyName, assemblyName);
	wcscpy_s(this->signatureText, signatureText);
}

FunctionInfo::~FunctionInfo()
{
   functionID = 0;
   classID = 0;
   moduleID = 0;
   token = 0;
   className[0] = L'\0';
   functionName[0] = L'\0';
   assemblyName[0] = L'\0';
}

FunctionInfo* FunctionInfo::GetNullObject()
{
   static FunctionInfo nullObjectFunctionInfo;
   return &nullObjectFunctionInfo;
}