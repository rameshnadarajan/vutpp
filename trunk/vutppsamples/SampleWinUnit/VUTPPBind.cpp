#include <windows.h>
#include <string>
#include "WinUnit.h"

// typedef void (CALLBACK FAR * TestFailureCB) (LPCSTR failure, int line);
typedef bool (__cdecl *TestPtr)(wchar_t* buffer, size_t cchBuffer);

OUT const std::string WCharToString( const wchar_t * wstr, int len )
{
	if( len == -1 )
		len = (int)wcslen(wstr);

	std::string strBuffer;
	int nCopied = WideCharToMultiByte(
		CP_OEMCP,
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
		CP_OEMCP,
		WC_COMPOSITECHECK,
		wstr,									// wide tstring
		len,								// length of wide tstring
		const_cast<LPSTR>(strBuffer.data()),	// mbcs tstring (unicode)
		nCopied,					// length of mbcs tstring
		NULL,									// NULL ¿Ã ∫ÅE£¥Ÿ¥¬µ?
		NULL );

	return strBuffer;
}

EXTERN_C __declspec(dllexport) void VUTPPBind( HMODULE hModule, HANDLE readPipe, HANDLE writePipe )
{
	char readBuffer[1024], writeBuffer[1024];

	sprintf( writeBuffer, "%d,%d\n", readPipe, writePipe );

	DWORD dwSize = 0;
	strcpy( writeBuffer, "connect" );
	if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
		return;

	while( true )
	{
		if( ReadFile( readPipe, readBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
			return;

		if( strncmp( readBuffer, "__VUTPP_FINISH__", 16 ) == 0 )
			break;

		const char* pSeperator = strchr( readBuffer, ',' );
		std::string suiteName( readBuffer, pSeperator - readBuffer ), testName( pSeperator+1 );

		wchar_t buffer[1024] = L"";

		TestPtr testPtr = (TestPtr)::GetProcAddress(
			hModule, ( "TEST_" + testName ).c_str());

		if (testPtr == NULL) 
		{ 
			sprintf( writeBuffer, "%d,,%s", -2, "can't find test" );
			if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
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
			sprintf( writeBuffer, "%d,,%s", line, failure.c_str() );
			if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
				return;
		}
		strcpy( writeBuffer, "-1," );
		if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
			return;
	}
}
