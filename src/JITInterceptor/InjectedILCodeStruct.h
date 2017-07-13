#pragma once

#include <windows.h>

typedef struct
{
	BYTE nop;
	BYTE callOp;
	BYTE callToken[4];
} ZeroArgsMethodILCode;

typedef struct {
	BYTE ldOp0;
	BYTE ldOp;
	BYTE fieldToken[4];
	BYTE callOp;
	BYTE callToken[4];
} OneArgsMethodILCode;

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE ldstr3;
	BYTE stringToken3[4];
	BYTE call;
	BYTE callToken[4];
} InterceptMethodBeginIL1;

typedef struct {
	BYTE ldstr1;
	BYTE stringToken1[4];
	BYTE ldstr2;
	BYTE stringToken2[4];
	BYTE ldOp1;
	BYTE ldOp2;
	BYTE fieldToken3[4];
	BYTE call;
	BYTE callToken[4];
} InterceptMethodBeginIL2;

typedef struct {
	BYTE nop;
	BYTE ldstr;
	BYTE stringToken[4];
	BYTE call;
	BYTE callToken[4];
} TestMethodILCode;