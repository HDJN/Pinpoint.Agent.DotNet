#pragma once
#include "ILWriterBase.h"

class SmallILWriter : public ILWriterBase
{
	friend class ILWriterBase;
	friend class Interceptor;

private:
	SmallILWriter(ICorProfilerInfo *profilerInfo, FunctionInfo *functionInfo, LPCBYTE oldMethodBytes, ULONG oldMethodSize, ULONG newMethodSize);
	~SmallILWriter(void);

	virtual ULONG GetHeaderSize();
	virtual ULONG GetOldMethodBodySize();
	virtual void WriteHeader(void* newMethodBytes);
	virtual void WriteExtra(void* newMethodBytes, int interceptBeforeILSize);
};
