#include <windows.h>
#include <string>
#include "TestHarness.h"

class VUTPP_Result : public TestResult
{
private:
	HANDLE m_WritePipe;

public:
	VUTPP_Result(HANDLE writePipe) : m_WritePipe(writePipe){}
	~VUTPP_Result() {}

	void addFailure (const Failure & failure)
	{
		char writeBuffer[1024];
		sprintf( writeBuffer, "%d,%s,%s", failure.lineNumber, failure.fileName.asCharString(), failure.message.asCharString() );
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

				Test* pTest = TestRegistry::Tests();
				while( pTest != NULL )
				{
					if( strcmp( pTest->name(), testName.c_str() ) == 0 )
					{
						VUTPP_Result testResult( writePipe );
						pTest->run(testResult);

						strcpy( writeBuffer, "-1," );
						bRun = true;
						if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
							return -1;
					}

					pTest = pTest->getNext();
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

	TestResult tr;
	return TestRegistry::runAllTests(tr);
}

