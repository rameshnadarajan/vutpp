#include <windows.h>
#include "UnitTest++.h"
#include "TestReporter.h"

using namespace UnitTest;

typedef void (CALLBACK FAR * TestFailureCB) (LPCSTR failure, int line);

class VUTPP_Reporter : public TestReporter
{
private:
	TestFailureCB m_CB;

public:
	VUTPP_Reporter(TestFailureCB CB) : m_CB( CB ){}
	~VUTPP_Reporter() {}

	void ReportTestStart(TestDetails const& test) override {}
	void ReportFailure(TestDetails const& test, char const* failure) override { m_CB( failure, test.lineNumber ); }
	void ReportTestFinish(TestDetails const& test, float secondsElapsed) override {}
	void ReportSummary(int totalTestCount, int failedTestCount, int failureCount, float secondsElapsed) override {}
};

EXTERN_C __declspec(dllexport) void RunTest( LPCSTR pSuiteName, LPCSTR pTestName, TestFailureCB CB )
{
	if( pTestName == NULL || pSuiteName == NULL || CB == NULL )
	{
		CB( "", -1 );
		return;
	}

	TestList& rTestList = Test::GetTestList();
	const Test* pTest = rTestList.GetHead();
	while( pTest != NULL )
	{
		if( strcmp( pTest->m_details.testName, pTestName ) == 0 && strcmp( pTest->m_details.suiteName, pSuiteName ) == 0 )
		{
			VUTPP_Reporter reporter( CB );
			TestResults testResult( &reporter );
			pTest->Run(testResult);
			return;
		}

		pTest = pTest->next;
	}

	CB( "", -1 );
	return;
}