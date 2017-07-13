#pragma once
#include "FunctionInfo.h"
#include "corhlpr.h"
#include "InjectedILCodeStruct.h"

#define FAT_HEADER_SIZE sizeof(WORD) + sizeof(WORD) + sizeof(DWORD) + sizeof(DWORD)
#define SMALL_HEADER_SIZE sizeof(BYTE)
#define MAX_TINY_FORMAT_SIZE 64

class ILWriterBase
{
protected:
	int ilCodeSize;

public:
	ILWriterBase(ICorProfilerInfo *profilerInfo, FunctionInfo *functionInfo, LPCBYTE oldMethodBytes, ULONG oldMethodSize, ULONG newMethodSize);
	~ILWriterBase(void);

	virtual BOOL CanRewrite();
	void *GetNewILBytes(void *interceptBeforeIL, int interceptBeforeILSize, void *interceptAfterIL, int interceptAfterILSzie, int afterILOffset);

protected:
	virtual ULONG GetOldMethodBodySize() = 0;
	virtual ULONG GetNewMethodSize();
	virtual ULONG GetHeaderSize() = 0;
	virtual ULONG GetOldHeaderSize();
	virtual void WriteHeader(void* newMethodBytes) = 0;
	virtual void WriteExtra(void* newMethodBytes, int interceptBeforeILSize) = 0;

	void *AllocateNewMethodBody(FunctionInfo *functionInfo);
	void *WriteILBody(void* newMethodBytes, void *interceptBeforeIL, int interceptBeforeILSize, void *interceptAfterIL, int interceptAfterILSzie, int afterILOffset);
	LPCBYTE GetOldMethodBytes();
	ULONG GetOldMethodSize();
	ULONG GetNewMethodBodySize();

protected:
	int ILCodeParse(BYTE *methodBody, int offset, BYTE *op, int *opLength, BYTE *arg, int *argLength);

	ICorProfilerInfo *profilerInfo;
	FunctionInfo *functionInfo;
	LPCBYTE oldMethodBytes;
	ULONG oldMethodSize;

	static BOOL IsTiny(LPCBYTE methodBytes);
};
