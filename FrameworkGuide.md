

# Framework Common #
  * Add bind codes to your project from samples

# BoostTest #
  * compiler.PreprocessorDefinitions need "VUTPP\_BOOSTTEST"
  * main suite name must "DefaultSuite" - use '#define BOOST\_TEST\_MODULE DefaultSuite'

# UnitTest++ #
  * framework home : http://sourceforge.net/projects/unittest-cpp/
  * compiler.PreprocessorDefinitions need "VUTPP\_UNITTEST++"

# CppUnitLite2 #
  * framework home : http://www.gamesfromwithin.com/articles/0512/000103.html
  * compiler.PreprocessorDefinitions need "VUTPP\_CPPUNITLITE2"
  * Fix library codes

```
- TestRegistry.h
int TestCount() { return m_testCount; }
Test** Tests() { return m_tests; } 
- Failure.h 
int LineNumber() const { return m_lineNumber; }
- Test.h 
const char *Name() const { return m_name; }
```

# CppUnitLite #
  * framework home : http://c2.com/cgi/wiki?CppUnitLite
  * compiler.PreprocessorDefinitions need "VUTPP\_CPPUNITLITE"
  * Convert CppUnitLite library to static lib or dynamic dll
  * Fix library codes

```
- TestRegistry.h  
static Test* Tests() { return instance().tests; } 
Test.h  
char* Name() { return name_.asCharString(); }  
```

# WinUnit #
  * framework home : http://msdn2.microsoft.com/ko-kr/magazine/cc136757.aspx
  * compiler.PreprocessorDefinitions need "VUTPP\_WINUNIT"

# GoogleTest #
  * framework home : http://googletest.googlecode.com
  * compiler.PreprocessorDefinitions need "VUTPP\_GOOGLETEST"

```
move class implementation of UnitTestEventListenerInterface to gtest.h from gtest.cc

add parse function header to testing::internal(gtest.h)

// gtest.h
void ParseGoogleTestFlagsOnly(int* argc, char** argv);
void ParseGoogleTestFlagsOnly(int* argc, wchar_t** argv);

Add AddListener function in UnitTest class

// gtest.h
void AddListener(class UnitTestEventListenerInterface *listener);

// gtest.cpp
void UnitTest::AddListener(UnitTestEventListenerInterface *listener)
{
        internal::MutexLock lock(&mutex_);
        static_cast<UnitTestEventsRepeater*>( impl_->result_printer() )->AddListener( listener );
}
```