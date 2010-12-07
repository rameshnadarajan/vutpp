

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 7.00.0555 */
/* at Wed Dec 08 00:15:14 2010
 */
/* Compiler settings for VUTPPUserControlHost.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 7.00.0555 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#if !defined(_M_IA64) && !defined(_M_AMD64)


#pragma warning( disable: 4049 )  /* more than 64k source lines */
#if _MSC_VER >= 1200
#pragma warning(push)
#endif

#pragma warning( disable: 4211 )  /* redefine extern to static */
#pragma warning( disable: 4232 )  /* dllimport identity*/
#pragma warning( disable: 4024 )  /* array to pointer mapping*/
#pragma warning( disable: 4152 )  /* function/data pointer conversion in expression */
#pragma warning( disable: 4100 ) /* unreferenced arguments in x86 call */

#pragma optimize("", off ) 

#define USE_STUBLESS_PROXY


/* verify that the <rpcproxy.h> version is high enough to compile this file*/
#ifndef __REDQ_RPCPROXY_H_VERSION__
#define __REQUIRED_RPCPROXY_H_VERSION__ 475
#endif


#include "rpcproxy.h"
#ifndef __RPCPROXY_H_VERSION__
#error this stub requires an updated version of <rpcproxy.h>
#endif /* __RPCPROXY_H_VERSION__ */


#include "VUTPPUserControlHost.h"

#define TYPE_FORMAT_STRING_SIZE   21                                
#define PROC_FORMAT_STRING_SIZE   37                                
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   0            

typedef struct _VUTPPUserControlHost_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } VUTPPUserControlHost_MIDL_TYPE_FORMAT_STRING;

typedef struct _VUTPPUserControlHost_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } VUTPPUserControlHost_MIDL_PROC_FORMAT_STRING;

typedef struct _VUTPPUserControlHost_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } VUTPPUserControlHost_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const VUTPPUserControlHost_MIDL_TYPE_FORMAT_STRING VUTPPUserControlHost__MIDL_TypeFormatString;
extern const VUTPPUserControlHost_MIDL_PROC_FORMAT_STRING VUTPPUserControlHost__MIDL_ProcFormatString;
extern const VUTPPUserControlHost_MIDL_EXPR_FORMAT_STRING VUTPPUserControlHost__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IVUTPPUserControlHostCtlCtl_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IVUTPPUserControlHostCtlCtl_ProxyInfo;



#if !defined(__RPC_WIN32__)
#error  Invalid build platform for this stub.
#endif

#if !(TARGET_IS_NT50_OR_LATER)
#error You need Windows 2000 or later to run this stub because it uses these features:
#error   /robust command line switch.
#error However, your C/C++ compilation flags indicate you intend to run this app on earlier systems.
#error This app will fail with the RPC_X_WRONG_STUB_VERSION error.
#endif


static const VUTPPUserControlHost_MIDL_PROC_FORMAT_STRING VUTPPUserControlHost__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure HostUserControl */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x7 ),	/* 7 */
/*  8 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 10 */	NdrFcShort( 0x0 ),	/* 0 */
/* 12 */	NdrFcShort( 0x8 ),	/* 8 */
/* 14 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 16 */	0x8,		/* 8 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 18 */	NdrFcShort( 0x0 ),	/* 0 */
/* 20 */	NdrFcShort( 0x0 ),	/* 0 */
/* 22 */	NdrFcShort( 0x0 ),	/* 0 */

	/* Parameter userControl */

/* 24 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 26 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 28 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 30 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 32 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 34 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const VUTPPUserControlHost_MIDL_TYPE_FORMAT_STRING VUTPPUserControlHost__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/*  4 */	NdrFcLong( 0x0 ),	/* 0 */
/*  8 */	NdrFcShort( 0x0 ),	/* 0 */
/* 10 */	NdrFcShort( 0x0 ),	/* 0 */
/* 12 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 14 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 16 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 18 */	0x0,		/* 0 */
			0x46,		/* 70 */

			0x0
        }
    };


/* Object interface: IUnknown, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: IDispatch, ver. 0.0,
   GUID={0x00020400,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: IVUTPPUserControlHostCtlCtl, ver. 0.0,
   GUID={0x27d3b582,0x2423,0x4736,{0xb8,0x69,0x3b,0xe6,0x35,0xb8,0xd2,0x72}} */

#pragma code_seg(".orpc")
static const unsigned short IVUTPPUserControlHostCtlCtl_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IVUTPPUserControlHostCtlCtl_ProxyInfo =
    {
    &Object_StubDesc,
    VUTPPUserControlHost__MIDL_ProcFormatString.Format,
    &IVUTPPUserControlHostCtlCtl_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IVUTPPUserControlHostCtlCtl_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    VUTPPUserControlHost__MIDL_ProcFormatString.Format,
    &IVUTPPUserControlHostCtlCtl_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(8) _IVUTPPUserControlHostCtlCtlProxyVtbl = 
{
    &IVUTPPUserControlHostCtlCtl_ProxyInfo,
    &IID_IVUTPPUserControlHostCtlCtl,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IDispatch::GetTypeInfoCount */ ,
    0 /* IDispatch::GetTypeInfo */ ,
    0 /* IDispatch::GetIDsOfNames */ ,
    0 /* IDispatch_Invoke_Proxy */ ,
    (void *) (INT_PTR) -1 /* IVUTPPUserControlHostCtlCtl::HostUserControl */
};


static const PRPC_STUB_FUNCTION IVUTPPUserControlHostCtlCtl_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2
};

CInterfaceStubVtbl _IVUTPPUserControlHostCtlCtlStubVtbl =
{
    &IID_IVUTPPUserControlHostCtlCtl,
    &IVUTPPUserControlHostCtlCtl_ServerInfo,
    8,
    &IVUTPPUserControlHostCtlCtl_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};

static const MIDL_STUB_DESC Object_StubDesc = 
    {
    0,
    NdrOleAllocate,
    NdrOleFree,
    0,
    0,
    0,
    0,
    0,
    VUTPPUserControlHost__MIDL_TypeFormatString.Format,
    1, /* -error bounds_check flag */
    0x50002, /* Ndr library version */
    0,
    0x700022b, /* MIDL Version 7.0.555 */
    0,
    0,
    0,  /* notify & notify_flag routine table */
    0x1, /* MIDL flag */
    0, /* cs routines */
    0,   /* proxy/server info */
    0
    };

const CInterfaceProxyVtbl * const _VUTPPUserControlHost_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_IVUTPPUserControlHostCtlCtlProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _VUTPPUserControlHost_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_IVUTPPUserControlHostCtlCtlStubVtbl,
    0
};

PCInterfaceName const _VUTPPUserControlHost_InterfaceNamesList[] = 
{
    "IVUTPPUserControlHostCtlCtl",
    0
};

const IID *  const _VUTPPUserControlHost_BaseIIDList[] = 
{
    &IID_IDispatch,
    0
};


#define _VUTPPUserControlHost_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _VUTPPUserControlHost, pIID, n)

int __stdcall _VUTPPUserControlHost_IID_Lookup( const IID * pIID, int * pIndex )
{
    
    if(!_VUTPPUserControlHost_CHECK_IID(0))
        {
        *pIndex = 0;
        return 1;
        }

    return 0;
}

const ExtendedProxyFileInfo VUTPPUserControlHost_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _VUTPPUserControlHost_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _VUTPPUserControlHost_StubVtblList,
    (const PCInterfaceName * ) & _VUTPPUserControlHost_InterfaceNamesList,
    (const IID ** ) & _VUTPPUserControlHost_BaseIIDList,
    & _VUTPPUserControlHost_IID_Lookup, 
    1,
    2,
    0, /* table of [async_uuid] interfaces */
    0, /* Filler1 */
    0, /* Filler2 */
    0  /* Filler3 */
};
#pragma optimize("", on )
#if _MSC_VER >= 1200
#pragma warning(pop)
#endif


#endif /* !defined(_M_IA64) && !defined(_M_AMD64)*/

