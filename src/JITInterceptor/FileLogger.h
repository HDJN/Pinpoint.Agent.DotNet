#pragma once

#include <Windows.h>
#include <ostream>
#include <sstream>
#include <fstream>
#include <iomanip>

class FileLogger : public std::ostream
{
private:
    class StreamBuffer: public std::stringbuf
    {
    public:
        StreamBuffer(std::ostream& str) : m_out(str), m_useOutputDebug(false) {};   
        StreamBuffer(std::ostream& str, std::string prefix) : m_out(str), m_prefix(prefix), m_useOutputDebug(false) {};
        ~StreamBuffer() {}
        void SetPrefix(const char* linePrefix)
        {
            m_prefix = std::string(linePrefix);
        }
        void Echo2OutputDebug(bool activate) { m_useOutputDebug = activate; }
        virtual int sync()
        {
            if (m_useOutputDebug)
            {       
                std::string tempStr = m_prefix + str();
                OutputDebugStringA(tempStr.c_str());
            }
            if (m_out.good())
            {
                m_out << str();
            }            
            str("");
            return 0;
        }
    private:
        bool m_useOutputDebug;
        std::ostream &m_out;
        std::string m_prefix;
    };

public:
    FileLogger(std::ostream& _ostream) : std::ostream(&m_streamBuffer), m_streamBuffer(_ostream) {}
    FileLogger(std::ostream& _ostream, std::string prefix) : std::ostream(&m_streamBuffer), m_streamBuffer(_ostream, prefix) {}
    FileLogger() : std::ostream(&m_streamBuffer), m_streamBuffer(m_file) {}
    ~FileLogger() { this->Close(); }
    bool IsActive() { return m_file.is_open() || m_useOutputDebug; }
    bool IsOpen() { return m_file.is_open(); }
    void Close();
    void Open(const char* fileName);        
    void WriteLine(const char* message);
    FileLogger& WriteHex64(__int64 val)
    {
        *this << hex << std::setw(8) << std::setfill('0') << val;
        return *this;
    }
    void Echo2OutputDebug(bool activate) { m_useOutputDebug = activate; m_streamBuffer.Echo2OutputDebug(activate); }
    void SetPrefix(const char* linePrefix);
private:
    bool m_useOutputDebug;
    std::ofstream m_file;
    std::string m_fileName;
    std::string m_prefix;
    StreamBuffer m_streamBuffer;
};
