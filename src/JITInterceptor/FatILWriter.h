#pragma once
#include "ILWriterBase.h"

class FatILWriter : public ILWriterBase
{
	friend class ILWriterBase;
	friend class Interceptor;

private:
	FatILWriter(ICorProfilerInfo *profilerInfo, FunctionInfo *functionInfo, LPCBYTE oldMethodBytes, ULONG oldMethodSize, ULONG newMethodSize);
	~FatILWriter(void);

	virtual ULONG GetHeaderSize();
	virtual ULONG GetOldMethodBodySize();
	virtual ULONG GetNewMethodSize();
	virtual void WriteHeader(void* newMethodBytes);
	virtual void WriteExtra(void* newMethodBytes, int interceptBeforeILSize);
	virtual BOOL CanRewrite();

	BOOL HasSEH();
	COR_ILMETHOD_FAT *GetFatInfo();
	ULONG GetExtraSectionsSize();
	ULONG GetOldDWORDAlignmentOffset();
	ULONG GetDWORDAlignmentOffset();
	ULONG GetOldMethodSizeWithoutExtraSections();
	void FixSEHSections(LPCBYTE methodBytes, ULONG newILSize);
};
