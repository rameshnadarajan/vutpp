

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 6.00.0361 */
/* at Thu Mar 12 15:35:27 2009
 */
/* Compiler settings for .\VUTPPUserControlHost.idl:
    Oicf, W1, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __VUTPPUserControlHost_h__
#define __VUTPPUserControlHost_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IVUTPPUserControlHostCtlCtl_FWD_DEFINED__
#define __IVUTPPUserControlHostCtlCtl_FWD_DEFINED__
typedef interface IVUTPPUserControlHostCtlCtl IVUTPPUserControlHostCtlCtl;
#endif 	/* __IVUTPPUserControlHostCtlCtl_FWD_DEFINED__ */


#ifndef __VUTPPUserControlHostCtl_FWD_DEFINED__
#define __VUTPPUserControlHostCtl_FWD_DEFINED__

#ifdef __cplusplus
typedef class VUTPPUserControlHostCtl VUTPPUserControlHostCtl;
#else
typedef struct VUTPPUserControlHostCtl VUTPPUserControlHostCtl;
#endif /* __cplusplus */

#endif 	/* __VUTPPUserControlHostCtl_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 

void * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void * ); 

#ifndef __IVUTPPUserControlHostCtlCtl_INTERFACE_DEFINED__
#define __IVUTPPUserControlHostCtlCtl_INTERFACE_DEFINED__

/* interface IVUTPPUserControlHostCtlCtl */
/* [unique][helpstring][nonextensible][dual][uuid][object] */ 


EXTERN_C const IID IID_IVUTPPUserControlHostCtlCtl;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("27d3b582-2423-4736-b869-3be635b8d272")
    IVUTPPUserControlHostCtlCtl : public IDispatch
    {
    public:
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE HostUserControl( 
            IUnknown *userControl) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IVUTPPUserControlHostCtlCtlVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IVUTPPUserControlHostCtlCtl * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IVUTPPUserControlHostCtlCtl * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IVUTPPUserControlHostCtlCtl * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IVUTPPUserControlHostCtlCtl * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IVUTPPUserControlHostCtlCtl * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IVUTPPUserControlHostCtlCtl * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IVUTPPUserControlHostCtlCtl * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *HostUserControl )( 
            IVUTPPUserControlHostCtlCtl * This,
            IUnknown *userControl);
        
        END_INTERFACE
    } IVUTPPUserControlHostCtlCtlVtbl;

    interface IVUTPPUserControlHostCtlCtl
    {
        CONST_VTBL struct IVUTPPUserControlHostCtlCtlVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IVUTPPUserControlHostCtlCtl_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IVUTPPUserControlHostCtlCtl_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IVUTPPUserControlHostCtlCtl_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IVUTPPUserControlHostCtlCtl_GetTypeInfoCount(This,pctinfo)	\
    (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo)

#define IVUTPPUserControlHostCtlCtl_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo)

#define IVUTPPUserControlHostCtlCtl_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)

#define IVUTPPUserControlHostCtlCtl_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)


#define IVUTPPUserControlHostCtlCtl_HostUserControl(This,userControl)	\
    (This)->lpVtbl -> HostUserControl(This,userControl)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IVUTPPUserControlHostCtlCtl_HostUserControl_Proxy( 
    IVUTPPUserControlHostCtlCtl * This,
    IUnknown *userControl);


void __RPC_STUB IVUTPPUserControlHostCtlCtl_HostUserControl_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IVUTPPUserControlHostCtlCtl_INTERFACE_DEFINED__ */



#ifndef __VUTPPUserControlHostLib_LIBRARY_DEFINED__
#define __VUTPPUserControlHostLib_LIBRARY_DEFINED__

/* library VUTPPUserControlHostLib */
/* [helpstring][version][uuid] */ 


EXTERN_C const IID LIBID_VUTPPUserControlHostLib;

EXTERN_C const CLSID CLSID_VUTPPUserControlHostCtl;

#ifdef __cplusplus

class DECLSPEC_UUID("fd5177df-46de-4c6b-9588-9d1590b3525c")
VUTPPUserControlHostCtl;
#endif
#endif /* __VUTPPUserControlHostLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


