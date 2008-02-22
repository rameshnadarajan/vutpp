#include <windows.h>

#define BOOST_TEST_MODULE DefaultSuite
#include <boost/test/floating_point_comparison.hpp>
#include <boost/test/unit_test.hpp>

BOOST_AUTO_TEST_SUITE(SampleSuite);

BOOST_AUTO_TEST_CASE(SAMPLE_TEST)
{
	//Sleep( 1000 );
	int a = 3;
	BOOST_CHECK_EQUAL( a, 3 );
}

BOOST_AUTO_TEST_CASE(SAMPLE_TEST2)
{
	//Sleep( 1000 );
	float a = 3.f;

	BOOST_CHECK_CLOSE( a, 2.f, 0.1f );
}

BOOST_AUTO_TEST_CASE(SAMPLE_TEST3)
{
	//Sleep( 1000 );
	int a = 3;
	BOOST_CHECK( a==3 );
}

BOOST_AUTO_TEST_SUITE_END();

BOOST_AUTO_TEST_CASE(SAMPLE_TEST)
{
	//Sleep( 1000 );
	int a = 3;
	BOOST_CHECK_EQUAL( a, 2 );
}

