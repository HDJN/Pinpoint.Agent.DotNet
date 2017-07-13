#include "stdafx.h"
#include "StringHelper.h"
#include <sstream>
#include <string>
#include <list>

std::string ConvertStlString(std::wstring& text)
{    
   const std::string narrow(text.begin(), text.end());
   return narrow;
}

std::wstring ConvertStlString(std::string& text)
{
   const std::wstring wide(text.begin(), text.end());
   return wide;
}

std::string PrintHex(LPCBYTE bytes, ULONG count)
{
   std::stringstream s;
   s << "[";
   for (ULONG i=0; i < count; i++)
   {
      char buffer[10];
      unsigned int hexval = bytes[i];
      sprintf_s(buffer, "%02X", hexval);
      s << buffer;
   }
   s << "]=";
   s << count;
   return s.str();
}

std::list<std::wstring> Split(const std::wstring &str, const std::wstring &pattern)
{
	std::list<std::wstring> ret;
	if (pattern.empty()) return ret;
	size_t start = 0, index = str.find_first_of(pattern, 0);
	while (index != str.npos)
	{
		if (start != index)
			ret.push_back(str.substr(start, index - start));
		start = index + 1;
		index = str.find_first_of(pattern, start);
	}
	if (!str.substr(start).empty())
		ret.push_back(str.substr(start));
	return ret;
}