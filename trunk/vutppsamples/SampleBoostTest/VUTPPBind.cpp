#include <boost/test/unit_test.hpp>
#include <windows.h>

#include <boost/test/unit_test_log_formatter.hpp>

using namespace boost::unit_test;

::boost::unit_test::test_suite*
init_unit_test_suite( int argc, char* argv[] )
{
	return 0;
}
struct test_runner : public test_tree_visitor {
	test_runner( const std::string& rSuiteName, const std::string& rTestName, const HANDLE vWritePipe ) : m_SuiteName( rSuiteName ), m_TestName( rTestName ) {}

	void        visit( test_case const& test )
	{
		test_suite const& rSuite = framework::get<test_suite>( test.p_parent_id );
		if( rSuite.p_name != m_SuiteName )
			return;

		if( test.p_name->size() == m_TestName.size() )
		{
			if( test.p_name.get() != m_TestName )
				return;
		}
		else if( strncmp( test.p_name.get().c_str(), m_TestName.c_str(), m_TestName.size() ) != 0 || test.p_name.get().at(m_TestName.size()) != '<' )
			return;

		tests.push_back( &test );
	}

	bool bRun;
	std::string m_SuiteName, m_TestName;
	typedef std::list<const test_case*> TEST_CONTAINER;
	TEST_CONTAINER tests;
};

class vutpp_log_formatter : public unit_test_log_formatter {
public:
	vutpp_log_formatter(HANDLE writePipe) : m_WritePipe( writePipe ), m_line_num(-1) {}

	void    log_start( std::ostream&, counter_t test_cases_amount ) {}
	void    log_finish( std::ostream& ) {}
	void    log_build_info( std::ostream& ) {}

	void    test_unit_start( std::ostream&, test_unit const& tu ) {}
	void    test_unit_finish( std::ostream&, test_unit const& tu, unsigned long elapsed ) {}
	void    test_unit_skipped( std::ostream&, test_unit const& tu ) {}

	void    log_exception( std::ostream&, log_checkpoint_data const& cpd, const_string explanation )
	{
		if( cpd.m_line_num >= 0 )
		{
			char writeBuffer[1024];
			sprintf( writeBuffer, "%d,%s,%s", cpd.m_line_num, cpd.m_file_name.begin(), explanation.begin() );
			DWORD dwWrite;
			if( WriteFile( m_WritePipe, writeBuffer, 1024, &dwWrite, NULL ) == false || dwWrite != 1024 )
				ExitProcess(-1);
		}
	}

	void    log_entry_start( std::ostream&, log_entry_data const& entry_data, log_entry_types let ) 
	{ m_line_num = (int)entry_data.m_line_num; m_filename = entry_data.m_file_name; }
	void    log_entry_value( std::ostream&, const_string value ) 
	{ m_Log += value.begin(); }
	void    log_entry_finish( std::ostream& )
	{
		if(m_line_num >= 0)
		{
			char writeBuffer[1024];
			if( test_name.empty() == false )
				sprintf( writeBuffer, "%d,%s,%s in %s", m_line_num, m_filename.c_str(), m_Log.c_str(), test_name.c_str() );
			else
				sprintf( writeBuffer, "%d,%s,%s", m_line_num, m_filename.c_str(), m_Log.c_str() );
			DWORD dwWrite;
			if( WriteFile( m_WritePipe, writeBuffer, 1024, &dwWrite, NULL ) == false || dwWrite != 1024 )
				ExitProcess(-1);
		}
		m_line_num = -1;
		m_Log.clear();
	}

	std::string test_name;
private:
	HANDLE m_WritePipe;
	std::string m_filename;
	int m_line_num;
	std::string m_Log;
};

int main(int argc, char *argv[])
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

			//			__asm { int 3 };

			framework::init( &init_unit_test_suite, 0, NULL );
			framework::master_test_suite().p_name.value = "DefaultSuite";
			vutpp_log_formatter* pLog = new vutpp_log_formatter(writePipe);
			unit_test_log.set_formatter( pLog );	
			while( true )
			{
				if( ReadFile( readPipe, readBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
					return -1;

				if( strncmp( readBuffer, "__VUTPP_FINISH__", 16 ) == 0 )
					break;

				const char* pSeperator = strchr( readBuffer, ',' );
				std::string suiteName( readBuffer, pSeperator - readBuffer ), testName( pSeperator+1 );

				test_runner runner( suiteName, testName, writePipe );
				traverse_test_tree( framework::master_test_suite().p_id, runner );

				if( runner.tests.empty() == false )
				{
					for( test_runner::TEST_CONTAINER::iterator itr = runner.tests.begin(), endItr = runner.tests.end(); itr != endItr; itr++ )
					{
						if( runner.m_TestName.size() != (*itr)->p_name->size() )
							pLog->test_name = (*itr)->p_name;
						else
							pLog->test_name.clear();
						framework::run( *itr, false );
					}

					strcpy( writeBuffer, "-1," );
					if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
						return -1;
				}
				else
				{
					sprintf( writeBuffer, "%d,,%s", -2, "can't find test" );
					if( WriteFile( writePipe, writeBuffer, 1024, &dwSize, NULL ) == false || dwSize != 1024 )
						return -1;
				}
			}

			return 0;
		}
	}

	return ::unit_test_main( &init_unit_test_suite, argc, argv );
}


