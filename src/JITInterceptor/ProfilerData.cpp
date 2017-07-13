#include "stdafx.h"

#include "ProfilerData.h"
#include "ProfilerLoggers.h"
#include "PerformanceCounter.h"
#include "SystemHelper.h"

using namespace std;

ProfilerData::ProfilerData()
{
	InitializeCriticalSection(&m_cs_debug);
	InitializeCriticalSection(&m_cs_output);

	m_debugLogger.SetPrefix("[JITInterceptor] ");
	m_debugLogger.Echo2OutputDebug(true);

	// g_debugLogger works even if the file is not open
	// It can signal errors when the file cannot be opened
	g_debugLogger.WriteLine("JITInterceptorFactory::JITInterceptorFactory()");

	std::string envFilename = std::string("PINPOINT_HOME");
	std::string debugFilename = GetEnvVar(envFilename) + "logs\\log.txt";

	if (debugFilename.size() != 0)
	{
		m_debugLogger.Open(debugFilename.c_str());
		if (!m_debugLogger.IsOpen())
		{
			g_debugLogger << "DebugLogger - Failed to open file: " << debugFilename.c_str() << endl;
		}
	}

	std::string envStacktrace = std::string("ILREWRITE_PROFILER_OUTPUT");
	std::string stackFilename = GetEnvVar(envStacktrace);
	if (stackFilename.size() != 0)
	{
		g_outputLogger.Open(stackFilename.c_str());
		if (!m_outputLogger.IsOpen())
		{
			g_debugLogger << "OutputLogger - Failed to open file: " << debugFilename.c_str() << endl;
		}
	}
	else
	{
		g_debugLogger.WriteLine("OutputLogger - A file must be specified");
		g_debugLogger.WriteLine("SET ILREWRITE_PROFILER_OUTPUT=<file>");
	}

	g_debugLogger.WriteLine("JITInterceptorFactory::JITInterceptorFactory()");

	PerformanceCounter perfCounter;
	g_debugLogger << "PERF - TicksPerSecond = " << dec << perfCounter.GetTicksPerSecond() << endl;
}

ProfilerData::~ProfilerData()
{
	//g_debugLogger << std::endl;
	//g_outputLogger << std::endl;
	DeleteCriticalSection(&m_cs_debug);
	DeleteCriticalSection(&m_cs_output);
}
