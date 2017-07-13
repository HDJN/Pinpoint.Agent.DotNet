#pragma once

#include <Windows.h>

class PerformanceCounter
{
public:
   PerformanceCounter()
   {
      if (!QueryPerformanceFrequency( &m_ticksPerSecond ))
         throw "QueryPerformanceFrequency failed";
   }

   void Start()
   {
      m_start.QuadPart = 0;
      QueryPerformanceCounter( &m_start );
   }

   void Stop()
   {
      m_end.QuadPart = 0;
      QueryPerformanceCounter( &m_end );
   }

   long double GetDurationInSeconds()
   {
      return (m_end.QuadPart - m_start.QuadPart) / long double( m_ticksPerSecond.QuadPart );
   }

   long long GetDurationInTicks()
   {
      return (m_end.QuadPart - m_start.QuadPart);
   }

   long long GetTicksPerSecond()
   {
      return m_ticksPerSecond.QuadPart;
   }

private:
   LARGE_INTEGER m_start;
   LARGE_INTEGER m_end;
   LARGE_INTEGER m_ticksPerSecond;
};
