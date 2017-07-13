#include "StdAfx.h"
#include "SmallILWriter.h"

SmallILWriter::SmallILWriter(ICorProfilerInfo *profilerInfo, FunctionInfo *functionInfo, LPCBYTE oldMethodBytes, ULONG oldMethodSize, ULONG newMethodSize)
	: ILWriterBase(profilerInfo, functionInfo, oldMethodBytes, oldMethodSize, newMethodSize)
{
}

SmallILWriter::~SmallILWriter(void)
{
}

ULONG SmallILWriter::GetHeaderSize()
{
	return SMALL_HEADER_SIZE;
}

ULONG SmallILWriter::GetOldMethodBodySize()
{
	return ((COR_ILMETHOD_TINY*)GetOldMethodBytes())->GetCodeSize();
}

void SmallILWriter::WriteHeader(void* newMethodBytes)
{
	BYTE newCodeSizeWithoutHeader = (GetNewMethodBodySize() << 2) | CorILMethod_TinyFormat;
	memcpy((BYTE*)newMethodBytes, &newCodeSizeWithoutHeader, GetHeaderSize());
}

void SmallILWriter::WriteExtra(void *newMethodBytes, int interceptBeforeILSize)
{

}