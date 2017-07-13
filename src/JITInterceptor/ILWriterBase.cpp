#include "StdAfx.h"
#include "ILWriterBase.h"
#include "SmallILWriter.h"
#include "FatILWriter.h"
#include "ConvertedFatILWriter.h"
#include "ProfilerLoggers.h"

#define Check(hr) if (FAILED(hr)) exit(1);

ILWriterBase::ILWriterBase(ICorProfilerInfo *profilerInfo, FunctionInfo *functionInfo, LPCBYTE oldMethodBytes, ULONG oldMethodSize, ULONG newMethodSize)
{
	ILWriterBase::profilerInfo = profilerInfo;
	ILWriterBase::functionInfo = functionInfo;
	ILWriterBase::oldMethodBytes = oldMethodBytes;
	ILWriterBase::oldMethodSize = oldMethodSize;
	this->ilCodeSize = newMethodSize;
}

ILWriterBase::~ILWriterBase(void)
{
}

BOOL ILWriterBase::IsTiny(LPCBYTE methodBytes)
{
	return ((COR_ILMETHOD_TINY*)methodBytes)->IsTiny();
}

LPCBYTE ILWriterBase::GetOldMethodBytes()
{
	return oldMethodBytes;
}

ULONG ILWriterBase::GetOldMethodSize()
{
	return oldMethodSize;
}

ULONG ILWriterBase::GetOldHeaderSize()
{
	return GetHeaderSize();
}

ULONG ILWriterBase::GetNewMethodSize()
{
	return GetOldMethodSize() + ilCodeSize;
}

ULONG ILWriterBase::GetNewMethodBodySize()
{
	return GetOldMethodBodySize() + ilCodeSize;
}

BOOL ILWriterBase::CanRewrite()
{
	return TRUE;
}

void *ILWriterBase::GetNewILBytes(void *interceptBeforeIL, int interceptBeforeILSize, void *interceptAfterIL, int interceptAfterILSzie, int afterILOffset)
{
	void* newMethodBytes = AllocateNewMethodBody(functionInfo);

	/*int preOffset, offset = 0, opLength = 0, argLength = 0;
	BYTE op[2], arg[8];
	g_debugLogger << "old method begin" << std::endl;
	while (offset < GetOldMethodBodySize())
	{
		offset = ILCodeParse((BYTE*)GetOldMethodBytes() + GetHeaderSize(), offset, op, &opLength, arg, &argLength);
		g_debugLogger << PrintHex(op, opLength) << " " << PrintHex(arg, argLength) << std::endl;
	}
	g_debugLogger << "old method end" << std::endl;*/

	WriteHeader(newMethodBytes);

	WriteILBody(newMethodBytes, interceptBeforeIL, interceptBeforeILSize, interceptAfterIL, interceptAfterILSzie, afterILOffset);

	WriteExtra(newMethodBytes, interceptBeforeILSize);

	/*g_debugLogger << "new method begin" << std::endl;
	offset = 0;
	while (offset < GetNewMethodBodySize())
	{
		offset = ILCodeParse((BYTE*)newMethodBytes + GetHeaderSize(), offset, op, &opLength, arg, &argLength);
		g_debugLogger << PrintHex(op, opLength) << " " << PrintHex(arg, argLength) << std::endl;
	}
	g_debugLogger << "new method end" << std::endl;*/

	/*g_debugLogger << "old method:" << PrintHex(GetOldMethodBytes(), GetOldMethodSize()) << std::endl;
	g_debugLogger << "new method:" << PrintHex((BYTE*)newMethodBytes, GetNewMethodSize()) << std::endl;*/

	return newMethodBytes;
}

void *ILWriterBase::AllocateNewMethodBody(FunctionInfo *functionInfo)
{
	IMethodMalloc* methodMalloc = NULL;
	Check(profilerInfo->GetILFunctionBodyAllocator(functionInfo->GetModuleID(), &methodMalloc));
	void *result = methodMalloc->Alloc(GetNewMethodSize());
	methodMalloc->Release();
	return result;
}

void *ILWriterBase::WriteILBody(void* newMethodBytes, void *interceptBeforeIL, int interceptBeforeILSize, void *interceptAfterIL, int interceptAfterILSize, int afterILOffset)
{
	IMetaDataEmit* metaDataEmit = NULL;
	Check(profilerInfo->GetModuleMetaData(functionInfo->GetModuleID(), ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit));

	memcpy((BYTE*)newMethodBytes + GetHeaderSize(), interceptBeforeIL, interceptBeforeILSize);
	memcpy((BYTE*)newMethodBytes + GetHeaderSize() + interceptBeforeILSize, (BYTE*)GetOldMethodBytes() + GetOldHeaderSize(), GetOldMethodBodySize());

	BYTE *tmp = new BYTE[afterILOffset];
	memcpy(tmp, (BYTE*)newMethodBytes + GetHeaderSize() + GetOldMethodBodySize() + interceptBeforeILSize - afterILOffset, afterILOffset);
	memcpy((BYTE*)newMethodBytes + GetHeaderSize() + GetOldMethodBodySize() + interceptBeforeILSize - afterILOffset, interceptAfterIL, interceptAfterILSize);
	memcpy((BYTE*)newMethodBytes + GetHeaderSize() + GetOldMethodBodySize() + interceptBeforeILSize - afterILOffset + interceptAfterILSize, tmp, afterILOffset);
}

int ILWriterBase::ILCodeParse(BYTE *methodBody, int offset, BYTE *op, int *opLength, BYTE *arg, int *argLength)
{
	//TODO: some time will throw stack overflow exception
	const int tokenSize = sizeof(mdToken);
	const int int8Size = 1;
	const int int32Size = 4;
	const int int64Size = 8;
	const int float32Size = 4;
	const int float64Size = 8;

	BYTE opCode = *(methodBody + offset);
	if ((0x00 <= opCode) && (opCode <= 0x0d)) { *opLength = 1; *argLength = 0; }
	else if ((0x0e <= opCode) && (opCode <= 0x13)) { *opLength = 1; *argLength = int8Size; }
	else if ((0x14 <= opCode) && (opCode <= 0x1e)) { *opLength = 1; *argLength = 0; }
	else if (0x1f == opCode) { *opLength = 1; *argLength = int8Size; }
	else if (0x20 == opCode) { *opLength = 1; *argLength = int32Size; }
	else if (0x21 == opCode) { *opLength = 1; *argLength = int64Size; }
	else if (0x22 == opCode) { *opLength = 1; *argLength = float32Size; }
	else if (0x23 == opCode) { *opLength = 1; *argLength = float64Size; }
	else if ((0x25 <= opCode) && (opCode <= 0x26)) { *opLength = 1; *argLength = 0; }
	else if ((0x27 <= opCode) && (opCode <= 0x29)) { *opLength = 1; *argLength = tokenSize; }
	else if (0x2a == opCode) { *opLength = 1; *argLength = 0; }
	else if ((0x2b <= opCode) && (opCode <= 0x37)) { *opLength = 1; *argLength = int8Size; }
	else if ((0x38 <= opCode) && (opCode <= 0x44)) { *opLength = 1; *argLength = int32Size; }
	else if (0x45 == opCode) { *opLength = 1; *argLength = 1 + *(methodBody + offset + 1); }
	else if ((0x46 <= opCode) && (opCode <= 0x6e)) { *opLength = 1; *argLength = 0; }
	else if ((0x6f <= opCode) && (opCode <= 0x75)) { *opLength = 1; *argLength = tokenSize; }
	else if ((0x76 <= opCode) && (opCode <= 0x7a)) { *opLength = 1; *argLength = 0; }
	else if (0x79 == opCode) { *opLength = 1; *argLength = tokenSize; }
	else if ((0x7b <= opCode) && (opCode <= 0x81)) { *opLength = 1; *argLength = tokenSize; }
	else if ((0x82 <= opCode) && (opCode <= 0x8b)) { *opLength = 1; *argLength = 0; }
	else if ((0x8c <= opCode) && (opCode <= 0x8d)) { *opLength = 1; *argLength = tokenSize; }
	else if (0x8e == opCode) { *opLength = 1; *argLength = 0; }
	else if (0x8f == opCode) { *opLength = 1; *argLength = tokenSize; }
	else if ((0x90 <= opCode) && (opCode <= 0xa2)) { *opLength = 1; *argLength = 0; }
	else if ((0xa3 <= opCode) && (opCode <= 0xa5)) { *opLength = 1; *argLength = tokenSize; }
	else if ((0xb3 <= opCode) && (opCode <= 0xba)) { *opLength = 1; *argLength = 0; }
	else if (0xc2 == opCode) { *opLength = 1; *argLength = tokenSize; }
	else if (0xc3 == opCode) { *opLength = 1; *argLength = 0; }
	else if (0xc6 == opCode) { *opLength = 1; *argLength = tokenSize; }
	else if (0xd0 == opCode) { *opLength = 1; *argLength = tokenSize; }
	else if ((0xd1 <= opCode) && (opCode <= 0xdc)) { *opLength = 1; *argLength = 0; }
	else if (0xdd == opCode) { *opLength = 1; *argLength = int32Size; }
	else if (0xde == opCode) { *opLength = 1; *argLength = int8Size; }
	else if (0xdf == opCode) { *opLength = 1; *argLength = 0; }
	else if (0xe0 == opCode) { *opLength = 1; *argLength = 0; }
	else if (0xfe == opCode)
	{
		BYTE extOpCode = *(methodBody + offset + 1);
		if ((0x00 <= extOpCode) && (extOpCode <= 0x05)) { *opLength = 2; *argLength = 0; }
		else if ((0x06 <= extOpCode) && (extOpCode <= 0x07)) { *opLength = 2; *argLength = tokenSize; }
		else if ((0x0f <= extOpCode) && (extOpCode <= 0x11)) { *opLength = 2; *argLength = 0; }
		else if (0x12 == extOpCode) { *opLength = 2; *argLength = int8Size; }
		else if ((0x13 <= extOpCode) && (extOpCode <= 0x14)) { *opLength = 2; *argLength = 0; }
		else if ((0x15 <= extOpCode) && (extOpCode <= 0x16)) { *opLength = 2; *argLength = tokenSize; }
		else if ((0x17 <= extOpCode) && (extOpCode <= 0x18)) { *opLength = 2; *argLength = 0; }
		else if (0x1a == extOpCode) { *opLength = 2; *argLength = 0; }
		else if (0x1c == extOpCode) { *opLength = 2; *argLength = sizeof(mdToken); }
		else if ((0x09 <= extOpCode) && (extOpCode <= 0x0e)) { *opLength = 2; *argLength = int32Size; }
		else if (0x1d == extOpCode) { *opLength = 2; *argLength = 0; }
		else if (0x1e == extOpCode) { *opLength = 2; *argLength = 0; }
		else { g_debugLogger << "Unrecognized opcode:" << PrintHex(&opCode, 1) << " " << PrintHex(&extOpCode, 1) << std::endl; }
	}
	else { g_debugLogger << "Unrecognized opcode:" << PrintHex(&opCode, 1) << std::endl; }

	memcpy(op, methodBody + offset, *opLength);
	memcpy(arg, methodBody + offset + *opLength, *argLength);
	return offset + *opLength + *argLength;
}