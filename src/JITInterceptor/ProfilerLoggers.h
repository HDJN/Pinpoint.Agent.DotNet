#pragma once

#include "ProfilerData.h"
#include "LockedStream.h"

extern ProfilerData g_profilerData;
#define g_debugLogger (LockedStream(g_profilerData.m_debugLogger, g_profilerData.m_cs_debug))
#define g_outputLogger (LockedStream(g_profilerData.m_outputLogger, g_profilerData.m_cs_output))
