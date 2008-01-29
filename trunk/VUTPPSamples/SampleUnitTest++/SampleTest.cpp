#include <windows.h>
#include "UnitTest++.h"

TEST( SAMPLE_TEST )
{
	Sleep( 1000 );
	int a = 3;
	CHECK_EQUAL( a, 3 );
}

TEST( SAMPLE_TEST2 )
{
	Sleep( 1000 );
	float a = 3.f;

	CHECK_CLOSE( a, 2.f, 0.1f );
}

TEST( SAMPLE_TEST3 )
{
	Sleep( 1000 );
	int a = 3;
	CHECK( a==3 );
}

SUITE( SampleSuite )
{
TEST( SampleSuite_SAMPLE_TEST )
{
	Sleep( 1000 );
	int a = 3;
	CHECK_EQUAL( a, 3 );
}
}
