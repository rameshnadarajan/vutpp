#include <windows.h>
#include <string>
#include "UnitTest++.h"
#include "TestReporter.h"

using namespace UnitTest;

class VUTPP_Reporter : public TestReporter
{
private:
	HANDLE m_WritePipe;

public:
	VUTPP_Reporter(HANDLE writePipe) : m_WritePipe(writePipe){}
	~VUTPP_Reporter() {}

	void ReportTestStart(TestDetails const& test) {}
	void ReportFailure(TestDetails const& test, char const* failure)
	{
		char writeBuffer[1024];
		sprintf( writeBuffer, "%d,%s,%s", test.lineNumber, test.filename, failure );
		DWORD dwWrite;
		if( WriteFile( m_WritePipe, writeBuffer, 1024, &dwWrite, NULL ) == false || dwWrite != 1024 )
			exit(-1);
	}
	void ReportTestFinish(TestDetails const& test, float secondsElapsed) {}
	void ReportSummary(int totalTestCount, int failedTestCount, int failureCount, float secondsElapsed) {}
};

int main(int argc, char const *argv[])
{
	for( int i = 1; i < argc; i++ )
	{
		if( strncmp( argv[i], "--vutpp:", 8 ) == 0 )
		{
			std::string strVutppParam = argv[i] + 8;
			const size_t seperator = strVutppParam.find( ',' );
			if( seperator == std::string::npos )
				return -1;

			HANDLE readPipe, writePipe;
			sscanf( strVutppParam.substr( 0, seperator ).c_str(), "%d", &readPipe );
			sscanf( strVutppParam.substr( seperator+1 ).c_str(), "%d", &writePipe );

			char readBuffer[1024], writeBuffer[1024];

			DWORD dwSize = 0;
			strcpy( writeBuffer, "connect" );
			if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
				return -1;

			TestList& rTestList = Test::GetTestList();

			while( true )
			{
				if( ReadFile( readPipe, readBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
					return -1;

				if( strncmp( readBuffer, "__VUTPP_FINISH__", 16 ) == 0 )
					break;

				const char* pSeperator = strchr( readBuffer, ',' );
				std::string suiteName( readBuffer, pSeperator - readBuffer ), testName( pSeperator+1 );

				bool bRun = false;
				Test* pTest = rTestList.GetHead();
				while( pTest != NULL )
				{
					if( pTest->m_details.testName == testName && pTest->m_details.suiteName == suiteName )
					{
						VUTPP_Reporter reporter( writePipe );
						TestResults testResult( &reporter );
						CurrentTest::Results() = &testResult;
						pTest->Run();
						strcpy( writeBuffer, "-1," );
						bRun = true;
						if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
							return -1;

						break;
					}

					pTest = pTest->next;
				}

				if( bRun == false )
				{
					sprintf( writeBuffer, "%d,,%s", -2, "can't find test" );
					if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
						return -1;
				}
			}

			return 0;
		}
	}

	return UnitTest::RunAllTests();
}
