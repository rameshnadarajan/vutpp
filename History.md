### Version 0.4 (2009/03/12) ###
  * Add support VisualStudio 2003
  * Add support GoogleTest
  * Change project base to console application from dll(exclude winunit)
  * convert back project to addin
  * Improve many codes.
  * Add many exception codes
  * change bind codes

### Version 0.3 (2008/03/13) ###
  * [issue 10](https://code.google.com/p/vutpp/issues/detail?id=10) : add support VisualStudio 2008
  * [issue 7](https://code.google.com/p/vutpp/issues/detail?id=7) : fix compiler.PreprocessorDefinitions problem
  * [issue 8](https://code.google.com/p/vutpp/issues/detail?id=8) : add support winunit
  * [issue 11](https://code.google.com/p/vutpp/issues/detail?id=11) : VUTPP cannot find TEST in Solution Folder
  * [issue 13](https://code.google.com/p/vutpp/issues/detail?id=13) : Add support shortcut-key
  * convert project to VSPackage
  * Improve ReparseCurrentFile(for Navigate and Update List) to use thread
  * Add MenuBar
  * change bind codes

### Version 0.2 (2008/01/15) ###
  * Vista가 아닌 OS에서 Icon이 제대로 나오지 않는 버그 수정(Shell32.dll의 아이콘을 사용하도록 했는데 OS별로 아이콘내용이 달라서 생기는 문제였음, famfamfam.com의 무료아이콘을 포함하도록 변경)
  * dll프로젝트가 아닌경우 run을 지원하지 않도록 수정
  * 주석외의 경우에 '/' 가 있을경우 그 이후의 코드가 인식되지 않는 버그 수정
  * Suite의 Icon 키 세팅이 되지 않아서 Run할 때 죽는 버그 수정
  * 실행중에 ActiveConfiguration을 변경하는 경우 죽는 버그 수정
  * RunTest의 return value가 정상적이지 않은 경우가 있어서 TestFailureCallback으로 feedback하도록 BindCode수정
  * boost auto test지원 추가 - 정재원(all2one)님께서 도와주셨습니다.

### Version 0.1 (2008/01/04) ###
  * First Release