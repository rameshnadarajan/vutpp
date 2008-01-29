#include <windows.h>
#include <boost/test/framework.hpp>
#include <boost/test/unit_test_log.hpp>
#include <boost/test/unit_test_log_formatter.hpp>
#include <boost/test/unit_test_suite_impl.hpp>

typedef void (CALLBACK FAR * TestFailureCB) (LPCSTR failure, int line);

namespace boost {

	namespace unit_test {

		namespace output {

			class BOOST_TEST_DECL vutpp_log_formatter : public unit_test_log_formatter {
			public:
				vutpp_log_formatter(TestFailureCB CB) : m_CB( CB ), m_line_num(-1) {}

				void    log_start( std::ostream&, counter_t test_cases_amount ) {}
				void    log_finish( std::ostream& ) {}
				void    log_build_info( std::ostream& ) {}

				void    test_unit_start( std::ostream&, test_unit const& tu ) {}
				void    test_unit_finish( std::ostream&, test_unit const& tu, unsigned long elapsed ) {};
				void    test_unit_skipped( std::ostream&, test_unit const& tu ) {}

				void    log_exception( std::ostream&, log_checkpoint_data const&, const_string explanation ) {}

				void    log_entry_start( std::ostream&, log_entry_data const& entry_data, log_entry_types let ) 
				{ if(let >= BOOST_UTL_ET_WARNING) m_line_num = (int)entry_data.m_line_num; }
				void    log_entry_value( std::ostream&, const_string value ) 
				{ m_Log += value.begin(); }
				void    log_entry_finish( std::ostream& )
				{ if(m_line_num >= 0) m_CB( m_Log.c_str(), m_line_num ); m_line_num = -1; m_Log.clear(); }

			private:
				TestFailureCB m_CB;
				int m_line_num;
				std::string m_Log;
			};

		} // namespace output

	} // namespace unit_test

} // namespace boost

using namespace boost::unit_test;

struct test_runner : test_tree_visitor {
	test_runner( const std::string& rSuiteName, const std::string& rTestName ) : m_SuiteName( rSuiteName ), m_TestName( rTestName ), m_bRun( false ) {}

	void        visit( test_case const& test )
	{
		if( test.p_name != m_TestName )
			return;

		test_suite const& rSuite = framework::get<test_suite>( test.p_parent_id );
		if( rSuite.p_name != m_SuiteName )
			return;

		framework::run( &test, false );

		m_bRun = true;
	}

	std::string m_SuiteName, m_TestName;
	bool m_bRun;
};

EXTERN_C __declspec(dllexport) void RunTest( LPCSTR pSuiteName, LPCSTR pTestName, TestFailureCB CB )
{
	if( pTestName == NULL || pSuiteName == NULL || CB == NULL )
	{
		CB( "", -1 );
		return;
	}

	framework::init( 0, NULL );

	unit_test_log.set_formatter( new output::vutpp_log_formatter(CB) );	

	test_runner runner( pSuiteName, pTestName );
	traverse_test_tree( framework::master_test_suite().p_id, runner );

	if( runner.m_bRun == false )
		CB( "", -1 );

	return;
}