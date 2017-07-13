#include "stdafx.h"
#include "FileLogger.h"

void FileLogger::Open(const char* fileName)
{
    if (m_file.is_open())
	{
		throw "FileLogger file is already open";
	}
	if (fileName == nullptr)
		throw "FileLogger(fileName=nullptr)";

    m_file.open(fileName, std::ios_base::out | std::ios_base::trunc);
    if (m_file.fail())
    {
        throw "FileLogger.Open - failed";
    }

	m_fileName = std::string(fileName);
}

void FileLogger::Close()
{
   m_streamBuffer.sync();
    if (m_file.is_open())
    {
        m_file.flush();
        m_file.close();
    }
}

void FileLogger::WriteLine(const char* message)
{
    *this << message << std::endl;
}

void FileLogger::SetPrefix(const char* linePrefix)
{
    m_streamBuffer.SetPrefix(linePrefix);
}
