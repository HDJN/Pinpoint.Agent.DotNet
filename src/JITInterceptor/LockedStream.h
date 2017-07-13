#pragma once

#include <Windows.h>
#include <ostream>
#include <sstream>
#include "FileLogger.h"
#include "StringHelper.h"

class LockedStream
{
public:
    void Close()
    {
        m_output.Close();
    }

    void Open(const char* fileName)
    {
        m_output.Open(fileName);
    }

    template <typename T>
    void WriteLine(T const& obj)
    {
        m_output << obj << std::endl;
    }

public:
    LockedStream(FileLogger& dest, CRITICAL_SECTION& cs) : m_output(dest), m_counter(0), m_cs(cs)
    {
        LPCRITICAL_SECTION local_cs = &m_cs;
        EnterCriticalSection(&m_cs);
        m_counter++;
    }

    LockedStream(LockedStream const& other) : m_output(other.m_output), m_cs(other.m_cs), m_counter(other.m_counter)
    {
        m_counter++;
    }

    ~LockedStream()
    {
        m_counter--;
        if (m_counter == 0)
        {

            LeaveCriticalSection(&m_cs);
        }
    }

    template< typename T >
    LockedStream& operator<<( T const& obj )
    {
        if (m_output.IsActive())
        {
            m_output << obj;
        }
        return *this;
    }

    typedef std::basic_ostream<char, std::char_traits<char> > CoutType;
    typedef CoutType& (*StandardEndLine)(CoutType&);
    LockedStream& LockedStream::operator<<(StandardEndLine manip)
    {
        if (m_output.IsActive())
        {
            m_output << manip;
        }
        return *this;
    }

    LockedStream& operator<<( std::ios& (*manip)( std::ios ))
    {
        if (m_output.IsActive())
        {
            m_output << manip;
        }
        return *this;
    }


    LockedStream& operator<<( std::ostream& (*manip)( std::ostream ) )
    {
        if (m_output.IsActive())
        {
            m_output << manip;
        }
        return *this;
    }

    LockedStream& operator<<( char const* s )
    {
        if (m_output.IsActive())
        {
            m_output << s;
        }
        return *this;
    }

    LockedStream& operator<<( wchar_t const* s )
    {
       if (m_output.IsActive())
       {
          std::wstring wide(s);
          std::string str = ConvertStlString(wide);
          m_output << str.c_str();
       }
       return *this;
    }

private:
    CRITICAL_SECTION& m_cs;
    FileLogger& m_output ;
    int m_counter ;
};