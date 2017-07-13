#pragma once
#include "ILWriterBase.h"

class ConvertedFatILWriter : public ILWriterBase
{
	friend class ILWriterBase;
	friend class Interceptor;

private:
	ConvertedFatILWriter(ICorProfilerInfo *profilerInfo, FunctionInfo *functionInfo, LPCBYTE oldMethodBytes, ULONG oldMethodSize, ULONG newMethodSize);
	~ConvertedFatILWriter(void);

	virtual ULONG GetHeaderSize();
	virtual ULONG GetOldHeaderSize();
	virtual ULONG GetOldMethodBodySize();
	virtual void WriteHeader(void* newMethodBytes);
	virtual void WriteExtra(void* newMethodBytes, int interceptBeforeILSize);
};
