#include "stdafx.h"

#include "SystemHelper.h"

const int MAX_BUFFER_SIZE = 500;

std::string GetEnvVar(std::string const& key)
{
   char buffer[MAX_BUFFER_SIZE];
   size_t returnSize = 0;
   getenv_s<MAX_BUFFER_SIZE>(&returnSize, buffer, key.c_str());
   return returnSize == 0 ? std::string("") : std::string(buffer, returnSize - 1);
}

std::wstring WGetEnvVar(const std::wstring &key)
{
	wchar_t buffer[MAX_BUFFER_SIZE];
	size_t returnSize = 0;
	_wgetenv_s<MAX_BUFFER_SIZE>(&returnSize, buffer, key.c_str());
	return returnSize == 0 ? std::wstring(L"") : std::wstring(buffer, returnSize - 1);
}
