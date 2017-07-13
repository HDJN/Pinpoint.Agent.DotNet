#pragma once

#include <string>
#include <list>

std::wstring ConvertStlString(std::string& text);
std::string ConvertStlString(std::wstring& text);
std::string PrintHex(LPCBYTE bytes, ULONG count);
std::list<std::wstring> Split(const std::wstring &str, const std::wstring &pattern);
