#include "StdAfx.h"
#include "ConvertedFatILWriter.h"

ConvertedFatILWriter::ConvertedFatILWriter(ICorProfilerInfo *profilerInfo, FunctionInfo *functionInfo, LPCBYTE oldMethodBytes, ULONG oldMethodSize, ULONG newMethodSize) :
	ILWriterBase(profilerInfo, functionInfo, oldMethodBytes, oldMethodSize, newMethodSize + 11)
{
}

ConvertedFatILWriter::~ConvertedFatILWriter(void)
{
}

ULONG ConvertedFatILWriter::GetHeaderSize()
{
	return FAT_HEADER_SIZE;
}

ULONG ConvertedFatILWriter::GetOldHeaderSize()
{
	return SMALL_HEADER_SIZE;
}

ULONG ConvertedFatILWriter::GetOldMethodBodySize()
{
	return ((COR_ILMETHOD_TINY*)GetOldMethodBytes())->GetCodeSize();
}

void ConvertedFatILWriter::WriteHeader(void* newMethodBytes)
{
	BYTE flags[] = { 0x13, 0x30, }; // not quite sure what this is. I think the "3" is CorILMethod_FatFormat
	WORD maxStackSize = 1 + ((ilCodeSize - 11) / 2); // rough estimation
	DWORD newSize = GetNewMethodBodySize() - 11;

	memcpy((BYTE*)newMethodBytes, &flags, sizeof(WORD));
	memcpy((BYTE*)newMethodBytes + sizeof(WORD), &maxStackSize, sizeof(WORD));
	memcpy((BYTE*)newMethodBytes + sizeof(WORD) + sizeof(WORD), &newSize, sizeof(DWORD));
}

void ConvertedFatILWriter::WriteExtra(void *newMethodBytes, int interceptBeforeILSize)
{

}