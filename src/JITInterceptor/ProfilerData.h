#pragma once

#include <Windows.h>
#include "FileLogger.h"

class ProfilerData
{
public:
   ProfilerData();
   ~ProfilerData();

   CRITICAL_SECTION m_cs_debug;
   CRITICAL_SECTION m_cs_output;

   FileLogger m_debugLogger;
   FileLogger m_outputLogger;
};