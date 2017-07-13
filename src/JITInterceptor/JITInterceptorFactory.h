#include <Unknwn.h>

class JITInterceptorFactory : public IClassFactory
{
public:
   JITInterceptorFactory();
   ~JITInterceptorFactory();

   //interface IUnknown methods 
   HRESULT __stdcall QueryInterface(const IID& iid, void **ppObj);
   ULONG   __stdcall AddRef();
   ULONG   __stdcall Release();

   //interface IClassFactory methods 
   HRESULT __stdcall CreateInstance(IUnknown* pUnknownOuter, const IID& iid, void** ppv);
   HRESULT __stdcall LockServer(BOOL bLock) ; 

private:
   long m_nRefCount;
};
