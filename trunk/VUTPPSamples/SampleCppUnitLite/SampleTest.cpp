#include <windows.h>
#include "TestHarness.h"

TEST( SAMPLE_TEST, GROUP )
{
	//Sleep( 1000 );
	int a = 3, b = 3;
	LONGS_EQUAL( a, b );
}

TEST( SAMPLE_TEST3, GROUP )
{
	//Sleep( 1000 );
	int a = 3, b = 2;
	LONGS_EQUAL( a, b );
	CHECK( a==3 );
}

