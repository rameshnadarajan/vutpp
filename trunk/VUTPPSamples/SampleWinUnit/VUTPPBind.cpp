#include <windows.h>
#include <string>
#include "WinUnit.h"

typedef void (CALLBACK FAR * TestFailureCB) (LPCSTR failure, int line);
typedef bool (__cdecl *TestPtr)(wchar_t* buffer, size_t cchBuffer);

OUT const std::string WCharToString( const wchar_t * wstr, int len )
{
	if( len == -1 )
		len = (int)wcslen(wstr);

	std::string strBuffer;
	int nCopied = WideCharToMultiByte(
		949,
		WC_COMPOSITECHECK,
		wstr,									// wide tstring
		len,								// length of wide tstring
		NULL,	// mbcs tstring (unicode)
		0,					// length of mbcs tstring
		NULL,									// NULL ¿Ã ∫ÅE£¥Ÿ¥¬µ?
		NULL );

	if( nCopied <= 0 )
		return strBuffer;

	strBuffer.resize( nCopied );

	WideCharToMultiByte(
		949,
		WC_COMPOSITECHECK,
		wstr,									// wide tstring
		len,								// length of wide tstring
		const_cast<LPSTR>(strBuffer.data()),	// mbcs tstring (unicode)
		nCopied,					// length of mbcs tstring
		NULL,									// NULL ¿Ã ∫ÅE£¥Ÿ¥¬µ?
		NULL );

	return strBuffer;
}

EXTERN_C __declspec(dllexport) void RunTest( HMODULE hModule, LPCSTR, LPCSTR pTestName, TestFailureCB CB )
{
	if( hModule == NULL || pTestName == NULL || CB == NULL )
	{
		CB( "", -1 );
		return;
	}

	std::string strTestName = "TEST_";
	strTestName += pTestName;

	wchar_t buffer[1024] = L"";

	if( hModule == NULL )
	{
		return;
	}
	TestPtr testPtr = (TestPtr)::GetProcAddress(
		hModule, strTestName.c_str());

	if (testPtr == NULL) 
	{ 
		CB( "", -1 );
		return;
	}

	// Execute the function
	bool bSuccess = (*testPtr)(buffer, ARRAYSIZE(buffer));

	if( bSuccess == false )
	{
		std::string failure = WCharToString(buffer, -1);
		size_t lineIndexBegin = failure.find( '(' );
		size_t lineIndexEnd = failure.find( ')' );
		int line = 0;

		if( lineIndexBegin != -1 && lineIndexEnd != -1 && lineIndexEnd > lineIndexBegin )
			line = atoi(failure.substr( lineIndexBegin+1, lineIndexEnd - lineIndexBegin ).c_str() );
		failure = failure.substr( failure.find( "failed." ) + 8 );
		CB( failure.c_str(), line );
	}

	return;
}