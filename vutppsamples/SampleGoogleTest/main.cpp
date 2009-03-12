#include <windows.h>
#include <gtest/gtest.h>
#include <gtest/gtest-test-part.h>

namespace testing
{
	class VUTPP_Listener : public UnitTestEventListenerInterface
	{
	private:
		HANDLE m_WritePipe;

	public:
		VUTPP_Listener(HANDLE writePipe) : m_WritePipe(writePipe){}
		~VUTPP_Listener() {}
		bool bRun;

		void OnNewTestPartResult(const TestPartResult* pResult)
		{
			bRun = true;
			if( pResult->type() == TPRT_SUCCESS )
				return;

			char messageTemp[1024];
			char writeBuffer[1024];
			strcpy( messageTemp, pResult->message() );
			char* pTemp = NULL;
			while( (pTemp = strchr( messageTemp, '\n' ) ) != NULL )
				*pTemp = ',';
			sprintf( writeBuffer, "%d,%s,%s", pResult->line_number(), pResult->file_name(), messageTemp );
			DWORD dwWrite;
			if( WriteFile( m_WritePipe, writeBuffer, 1024, &dwWrite, NULL ) == false || dwWrite != 1024 )
				exit(-1);
		}
	};
}

int main(int argc, char *argv[])
{
	testing::InitGoogleTest(&argc, argv);

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

			testing::VUTPP_Listener* pListener = new testing::VUTPP_Listener( writePipe );
			testing::UnitTest::GetInstance()->AddListener( pListener );

			while( true )
			{
				if( ReadFile( readPipe, readBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
					return -1;

				if( strncmp( readBuffer, "__VUTPP_FINISH__", 16 ) == 0 )
					break;

				const char* pSeperator = strchr( readBuffer, ',' );
				std::string suiteName( readBuffer, pSeperator - readBuffer ), testName( pSeperator+1 );

				char param[1000];
				char *params[2] = { NULL, param };
				int paramCount = 2;
				sprintf( param, "--gtest_filter=%s.%s", suiteName.c_str(), testName.c_str() );
				testing::internal::ParseGoogleTestFlagsOnly( &paramCount, params );

				pListener->bRun = false;
				RUN_ALL_TESTS();
				strcpy( writeBuffer, "-1," );
				if( pListener->bRun == false )
				{
					sprintf( writeBuffer, "%d,,%s", -2, "can't find test" );
					if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
						return -1;
				}
				else
				{
					if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
						return -1;
				}

			}

			return 0;
		}
	}

	return RUN_ALL_TESTS();
}
