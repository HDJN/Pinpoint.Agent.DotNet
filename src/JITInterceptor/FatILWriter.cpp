#include "StdAfx.h"
#include "FatILWriter.h"
extern "C" {
#include "corhlpr.h"
}

FatILWriter::FatILWriter(ICorProfilerInfo *profilerInfo, FunctionInfo *functionInfo, LPCBYTE oldMethodBytes, ULONG oldMethodSize, ULONG newMethodSize) :
	ILWriterBase(profilerInfo, functionInfo, oldMethodBytes, oldMethodSize, newMethodSize)
{
}

FatILWriter::~FatILWriter(void)
{
}

COR_ILMETHOD_FAT *FatILWriter::GetFatInfo()
{
	return (COR_ILMETHOD_FAT*)GetOldMethodBytes();
}

ULONG FatILWriter::GetHeaderSize()
{
	return FAT_HEADER_SIZE;
}

ULONG FatILWriter::GetOldMethodBodySize()
{
	return GetFatInfo()->GetCodeSize();
}

ULONG FatILWriter::GetNewMethodSize()
{
	return ILWriterBase::GetNewMethodSize() + GetDWORDAlignmentOffset();
}

BOOL FatILWriter::HasSEH()
{
	return (GetFatInfo()->GetFlags() & CorILMethod_MoreSects);
}

ULONG FatILWriter::GetExtraSectionsSize()
{
	return GetOldMethodSize() - GetOldMethodSizeWithoutExtraSections();
}

ULONG FatILWriter::GetOldMethodSizeWithoutExtraSections()
{
	return GetHeaderSize() + GetOldMethodBodySize();
}

ULONG FatILWriter::GetOldDWORDAlignmentOffset()
{
	if (!HasSEH())
	{
		return 0;
	}
	else
	{
		ULONG oldDelta = (int)((BYTE*)GetFatInfo()->GetSect() - (BYTE*)((BYTE*)GetOldMethodBytes() + GetOldMethodSizeWithoutExtraSections()));
		return oldDelta;
	}
}

ULONG FatILWriter::GetDWORDAlignmentOffset()
{
	if (!HasSEH())
	{
		return 0;
	}
	else
	{
		ULONG totalDelta = ((ilCodeSize + GetOldMethodSizeWithoutExtraSections()) % sizeof(DWORD));

		ULONG newDelta = 0;
		if (totalDelta != 0)
		{
			newDelta = sizeof(DWORD) - totalDelta;
		}

		return newDelta;
	}
}

BOOL FatILWriter::CanRewrite()
{
	if (HasSEH())
	{
		COR_ILMETHOD_DECODER method((const COR_ILMETHOD*)GetOldMethodBytes());
		COR_ILMETHOD_SECT_EH* currentEHSection = (COR_ILMETHOD_SECT_EH*)method.EH;

		do
		{
			for (UINT i = 0; i < currentEHSection->EHCount(); ++i)
			{
				if (!currentEHSection->IsFat() && ((currentEHSection->Small.Clauses[i].TryOffset + ilCodeSize) > 0xFFFF || (currentEHSection->Small.Clauses[i].HandlerOffset + ilCodeSize) > 0xFFFF))
				{
					return FALSE;
				}
			}

			do
			{
				currentEHSection = (COR_ILMETHOD_SECT_EH*)currentEHSection->Next();
			} while (currentEHSection && (currentEHSection->Kind() & CorILMethod_Sect_KindMask) != CorILMethod_Sect_EHTable);

		} while (currentEHSection);
	}

	return TRUE;
}

void FatILWriter::WriteHeader(void *newMethodBytes)
{
	WORD maxStackSize = (WORD)GetFatInfo()->MaxStack + (ilCodeSize / 2); // rough estimation
	DWORD newSize = GetNewMethodBodySize();

	memcpy((BYTE*)newMethodBytes, GetOldMethodBytes(), GetHeaderSize());
	memcpy((BYTE*)newMethodBytes + sizeof(WORD), &maxStackSize, sizeof(WORD));
	memcpy((BYTE*)newMethodBytes + sizeof(WORD) + sizeof(WORD), &newSize, sizeof(DWORD));
}

void FatILWriter::WriteExtra(void *newMethodBytes, int interceptBeforeILSize)
{
	BYTE zeroBytes[] = { 0x00, 0x00, 0x00 };
	memcpy((BYTE*)newMethodBytes + GetHeaderSize() + ilCodeSize + GetOldMethodBodySize(), zeroBytes, GetDWORDAlignmentOffset());
	memcpy((BYTE*)newMethodBytes + GetHeaderSize() + ilCodeSize + GetOldMethodBodySize() + GetDWORDAlignmentOffset(), (BYTE*)GetOldMethodBytes() + (GetOldMethodSize() - GetExtraSectionsSize() + GetOldDWORDAlignmentOffset()), GetExtraSectionsSize() - GetOldDWORDAlignmentOffset());

	if (HasSEH())
	{
		FixSEHSections((LPCBYTE)newMethodBytes, interceptBeforeILSize);
	}
}

void FatILWriter::FixSEHSections(LPCBYTE methodBytes, ULONG newILSize)
{
	COR_ILMETHOD_DECODER method((const COR_ILMETHOD*)methodBytes);
	COR_ILMETHOD_SECT_EH* currentEHSection = (COR_ILMETHOD_SECT_EH*)method.EH;

	do
	{
		for (UINT i = 0; i < currentEHSection->EHCount(); ++i)
		{
			if (currentEHSection->IsFat())
			{
				if (currentEHSection->Fat.Clauses[i].Flags == COR_ILEXCEPTION_CLAUSE_FILTER)
				{
					currentEHSection->Fat.Clauses[i].FilterOffset += newILSize;
				}

				currentEHSection->Fat.Clauses[i].TryOffset += newILSize;
				currentEHSection->Fat.Clauses[i].HandlerOffset += newILSize;
			}
			else
			{
				if (currentEHSection->Small.Clauses[i].Flags == COR_ILEXCEPTION_CLAUSE_FILTER)
				{
					currentEHSection->Small.Clauses[i].FilterOffset += newILSize;
				}

				currentEHSection->Small.Clauses[i].TryOffset += newILSize;
				currentEHSection->Small.Clauses[i].HandlerOffset += newILSize;
			}
		}

		do
		{
			currentEHSection = (COR_ILMETHOD_SECT_EH*)currentEHSection->Next();
		} while (currentEHSection && (currentEHSection->Kind() & CorILMethod_Sect_KindMask) != CorILMethod_Sect_EHTable);

	} while (currentEHSection);
}