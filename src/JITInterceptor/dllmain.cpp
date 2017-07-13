#include <windows.h>
#include <objbase.h>
#include "JITInterceptorFactory.h"
#include "JITInterceptor_h.h"

#include <stdio.h>

const int REG_STRING_MAX=512;

HMODULE g_hModule=NULL;
long g_comObjectsInUse=0;
const WCHAR JITInterceptorImplProgId[] = L"JITInterceptorLib.JITInterceptorImpl";


BOOL APIENTRY DllMain(HANDLE hModule,
   DWORD dwReason,
   void* lpReserved)
{
   if (dwReason == DLL_PROCESS_ATTACH)
   {
      g_hModule = (HMODULE)hModule ;
   }
   return TRUE ;
}


STDAPI DllGetClassObject(const CLSID& clsid,
   const IID& iid,
   void** ppv)
{    
   if (clsid == CLSID_JITInterceptorImpl)
   {
      JITInterceptorFactory* pJITInterceptorFactory = new JITInterceptorFactory();
      if (pJITInterceptorFactory == NULL)
         return E_OUTOFMEMORY;
      else
         return pJITInterceptorFactory->QueryInterface(iid , ppv);
   }
   return CLASS_E_CLASSNOTAVAILABLE;
}

STDAPI DllCanUnloadNow()
{
   if (g_comObjectsInUse == 0)
   {
      return S_OK;
   }
   else
   {
      return S_FALSE;
   }
}


BOOL HelperWriteKeyEx(
   HKEY roothk,
   const TCHAR *lpSubKey,
   LPCTSTR val_name, 
   DWORD dwType,
   void *lpvData, 
   DWORD dwDataSize);

BOOL HelperWriteKey(
   HKEY roothk,
   const TCHAR* subKey,
   const TCHAR* keyName,
   const TCHAR* keyValue)
{
   int dataSize = (wcslen(keyValue) + 1)*sizeof(TCHAR);
   return HelperWriteKeyEx(roothk, subKey, keyName, REG_SZ, (void*)keyValue, dataSize);
}


BOOL HelperWriteKeyEx(
   HKEY roothk,
   const TCHAR *lpSubKey,
   LPCTSTR val_name, 
   DWORD dwType,
   void *lpvData, 
   DWORD dwDataSize)
{
   HKEY hk;
   if (ERROR_SUCCESS != RegCreateKey(roothk,lpSubKey,&hk) ) return FALSE;

   if (ERROR_SUCCESS != RegSetValueEx(hk,val_name,0,dwType,(CONST BYTE *)lpvData,dwDataSize)) return FALSE;

   if (ERROR_SUCCESS != RegCloseKey(hk))   return FALSE;
   return TRUE;
}

HRESULT  __stdcall DllRegisterServer(void)
{
   WCHAR *lpwszClsid = NULL;
   WCHAR szBuff[REG_STRING_MAX] = { 0 };
   WCHAR szClsid[REG_STRING_MAX] = { 0 };
   WCHAR szInproc[REG_STRING_MAX] = { 0 };
   WCHAR szProgId[REG_STRING_MAX] = { 0 };
   WCHAR szDescriptionVal[REG_STRING_MAX] = { 0 };

   HRESULT hr = StringFromCLSID(CLSID_JITInterceptorImpl, &lpwszClsid);
   if (hr != S_OK)
      return hr;
   wsprintf(szClsid,L"%s",lpwszClsid);
   wsprintf(szInproc,L"%s\\%s\\%s",L"clsid",szClsid,L"InprocServer32");
   wsprintf(szProgId,L"%s\\%s\\%s",L"clsid",szClsid,L"ProgId");


   wsprintf(szBuff,L"%s",L"Fast Profiler");
   wsprintf(szDescriptionVal,L"%s\\%s",L"clsid",szClsid);

   HelperWriteKey (
      HKEY_CLASSES_ROOT,
      szDescriptionVal,
      NULL,
      szBuff
      );


   GetModuleFileName(
      g_hModule,
      szBuff,
      sizeof(szBuff));
   HelperWriteKey (
      HKEY_CLASSES_ROOT,
      szInproc,
      NULL,//write to the "default" value
      szBuff
      );

   lstrcpy(szBuff,JITInterceptorImplProgId);
   HelperWriteKey (
      HKEY_CLASSES_ROOT,
      szProgId,
      NULL,
      szBuff
      );


   wsprintf(szBuff,L"%s",L"Fast Profiler");
   HelperWriteKey (
      HKEY_CLASSES_ROOT,
      JITInterceptorImplProgId,
      NULL,
      szBuff
      );


   wsprintf(szProgId,L"%s\\%s",JITInterceptorImplProgId,L"CLSID");
   HelperWriteKey (
      HKEY_CLASSES_ROOT,
      szProgId,
      NULL,
      szClsid
      );

   return 1;
}

HRESULT  __stdcall DllUnregisterServer(void)
{
   WCHAR szKeyName[REG_STRING_MAX] = { 0 };
   WCHAR szClsid[REG_STRING_MAX] = { 0 };
   WCHAR *lpwszClsid = NULL;

   wsprintf(szKeyName,L"%s\\%s",JITInterceptorImplProgId,L"CLSID");
   RegDeleteKey(HKEY_CLASSES_ROOT,szKeyName);
   RegDeleteKey(HKEY_CLASSES_ROOT,JITInterceptorImplProgId);

   StringFromCLSID(
      CLSID_JITInterceptorImpl,
      &lpwszClsid);
   wsprintf(szClsid,L"%s",lpwszClsid);
   wsprintf(szKeyName,L"%s\\%s\\%s",L"CLSID",szClsid,L"InprocServer32");
   RegDeleteKey(HKEY_CLASSES_ROOT,szKeyName);

   wsprintf(szKeyName,L"%s\\%s\\%s",L"CLSID",szClsid,L"ProgId");
   RegDeleteKey(HKEY_CLASSES_ROOT,szKeyName);

   wsprintf(szKeyName,L"%s\\%s",L"CLSID",szClsid);
   RegDeleteKey(HKEY_CLASSES_ROOT,szKeyName);

   return 1;
}
