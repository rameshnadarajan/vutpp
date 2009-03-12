#include <windows.h>
#include "CppUnitLite2.h"
#include "TestResultStdErr.h"
#include <string>

class VUTPP_Result : public TestResult
{
private:
	HANDLE m_WritePipe;

public:
	VUTPP_Result(HANDLE writePipe) : m_WritePipe(writePipe){}
	~VUTPP_Result() {}

	void AddFailure (const Failure & failure)
	{
		char writeBuffer[1024];
		sprintf( writeBuffer, "%d,%s,%s", failure.LineNumber(), failure.Filename(), failure.Condition() );
		DWORD dwWrite;
		if( WriteFile( m_WritePipe, writeBuffer, 1024, &dwWrite, NULL ) == false || dwWrite != 1024 )
			exit(-1);
	}
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

			while( true )
			{
				if( ReadFile( readPipe, readBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
					return -1;

				if( strncmp( readBuffer, "__VUTPP_FINISH__", 16 ) == 0 )
					break;

				const char* pSeperator = strchr( readBuffer, ',' );
				std::string suiteName( readBuffer, pSeperator - readBuffer ), testName( pSeperator+1 );
				testName += "Test";

				bool bRun = false;

				for( int testIndex = 0; testIndex < TestRegistry::Instance().TestCount(); testIndex++ )
				{
					Test* pTest = TestRegistry::Instance().Tests()[testIndex];
					if( strcmp( pTest->Name(), testName.c_str() ) == 0 )
					{
						VUTPP_Result testResult( writePipe );
						pTest->Run(testResult);

						strcpy( writeBuffer, "-1," );
						bRun = true;
						if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
							return -1;
					}
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

	VUTPP_Result testResult( 0 );
	TestResultStdErr result;
	TestRegistry::Instance().Run(testResult);
	return (result.FailureCount());
}


