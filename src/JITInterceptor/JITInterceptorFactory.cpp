#include "stdafx.h"
#include "JITInterceptorFactory.h"
#include "JITInterceptorImpl.h"
#include "ProfilerLoggers.h"

using namespace std;

extern long g_comObjectsInUse;
ProfilerData g_profilerData;

JITInterceptorFactory::JITInterceptorFactory()
{
   m_nRefCount=0;
   InterlockedIncrement(&g_comObjectsInUse);
}

JITInterceptorFactory::~JITInterceptorFactory()
{
   g_debugLogger.WriteLine("JITInterceptorFactory::~JITInterceptorFactory");
   InterlockedDecrement(&g_comObjectsInUse);
}

HRESULT __stdcall JITInterceptorFactory::QueryInterface(
   const IID& iid, 
   void** ppv)
{   
   g_debugLogger.WriteLine("JITInterceptorFactory::QueryInterface");

   if ((iid == IID_IUnknown) || (iid == IID_IClassFactory))
   {
      *ppv = static_cast<IClassFactory*>(this) ; 
   }
   else
   {
      *ppv = NULL ;
      return E_NOINTERFACE ;
   }
   reinterpret_cast<IUnknown*>(*ppv)->AddRef() ;
   return S_OK ;
}

HRESULT __stdcall JITInterceptorFactory::CreateInstance(IUnknown* pUnknownOuter,
   const IID& iid,
   void** ppv) 
{
   g_debugLogger.WriteLine("JITInterceptorFactory::CreateInstance");
   if (!g_profilerData.m_outputLogger.IsOpen())
   {
      // DIAG_PRF_STACKTRACE must be defined
      // If not the stack will not be saved
      // It should have been opened in g_giagInit

      // Not true anymore
      // Added a server command to be able to set the stack log file
      // return E_INVALIDARG;
   }

   if (pUnknownOuter != NULL)
   {
      return CLASS_E_NOAGGREGATION ;
   }

   JITInterceptor* pObject = new JITInterceptorImpl();

   if (pObject == NULL)
   {
      return E_OUTOFMEMORY ;
   }

   return pObject->QueryInterface(iid, ppv) ;
}

ULONG __stdcall JITInterceptorFactory::AddRef()
{
   return InterlockedIncrement(&m_nRefCount) ;
}

ULONG __stdcall JITInterceptorFactory::Release()
{
   long nRefCount=0;
   nRefCount=InterlockedDecrement(&m_nRefCount) ;
   if (nRefCount == 0) delete this;
   return nRefCount;
}

HRESULT __stdcall JITInterceptorFactory::LockServer(BOOL bLock) 
{
   return E_NOTIMPL;
}