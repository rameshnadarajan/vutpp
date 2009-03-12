#include <windows.h>
#include "WinUnit.h"

BEGIN_TEST( SAMPLE_TEST )
{
	//Sleep( 1000 );
	int a = 3, b = 2;
	WIN_ASSERT_EQUAL( a, b );
}
END_TEST

FIXTURE( SampleFixture );
SETUP( SampleFixture ) {}
TEARDOWN( SampleFixture ) {}

BEGIN_TESTF( SAMPLE_TEST3, SampleFixture )
{
	//Sleep( 1000 );
	int a = 3, b = 2;
	WIN_ASSERT_EQUAL( a, b );
	WIN_ASSERT_TRUE( a==3 );
}
END_TESTF
