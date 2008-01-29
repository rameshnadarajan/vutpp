#include <windows.h>
#include <string>
#include "TestHarness.h"

typedef void (CALLBACK FAR * TestFailureCB) (LPCSTR failure, int line);

class VUTPP_Result : public TestResult
{
private:
	TestFailureCB m_CB;

public:
	VUTPP_Result(TestFailureCB CB) : m_CB( CB ){}
	~VUTPP_Result() {}

	void addFailure (const Failure & failure) override { m_CB( failure.message.asCharString(), failure.lineNumber ); }
};

EXTERN_C __declspec(dllexport) void RunTest( HMODULE, LPCSTR, LPCSTR pTestName, TestFailureCB CB )
{
	if( pTestName == NULL || CB == NULL )
	{
		CB( "", -1 );
		return;
	}

	std::string strTestName = pTestName;
	strTestName += "Test";

	Test* pTest = TestRegistry::Tests();
	while( pTest != NULL )
	{
		if( strcmp( pTest->name(), strTestName.c_str() ) == 0 )
		{
			VUTPP_Result testResult( CB );
			pTest->run(testResult);
			return;
		}

		pTest = pTest->getNext();
	}

	CB( "", -1 );
	return;
}