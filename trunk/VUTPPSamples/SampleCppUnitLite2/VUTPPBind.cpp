#include <windows.h>
#include "CppUnitLite2.h"
#include <string>

typedef void (CALLBACK FAR * TestFailureCB) (LPCSTR failure, int line);

class VUTPP_Result : public TestResult
{
private:
	TestFailureCB m_CB;

public:
	VUTPP_Result(TestFailureCB CB) : m_CB( CB ){}
	~VUTPP_Result() {}

	void AddFailure (const Failure & failure) override { m_CB( failure.Condition(), failure.LineNumber() ); }
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

	for( int testIndex = 0; testIndex < TestRegistry::Instance().TestCount(); testIndex++ )
	{
		Test* pTest = TestRegistry::Instance().Tests()[testIndex];

		if( strcmp( pTest->Name(), strTestName.c_str() ) == 0 )
		{
			VUTPP_Result testResult( CB );
			pTest->Run(testResult);
			return;
		}
	}

	CB( "", -1 );
	return;
}