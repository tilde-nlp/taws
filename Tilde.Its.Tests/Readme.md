Tilde.Its.Tests
===============

This project contains unit tests for the Tilde ITS implementation.

ITS 2.0 Test Suite
------------------

Tests from the official ITS 2.0 test suite are used in the project.
https://github.com/finnle/ITS-2.0-Testsuite 

The test suite uses the [W3C Test Suite Licence](http://www.w3.org/Consortium/Legal/2008/04-testsuite-license.html).

The files from the test suite (in the `its2.0` directory) are located in `TestData\TestSuite`.

When the tests are run, the test suite files are read from the current directory. For this reason, these files should be copied to the output directory as well. It can be automated by setting the following properties in Visual Studio:
  
  * Build Action: None
  * Copy to Output Directory: Copy if newer

Unit tests themselves can be found in the `Tests\TestSuite` directory and reside in the `Tilde.Its.Tests.TestSuite` namespace.

Unit tests are static and are not generated because MSTest doesn't support it. When new tests are added to the test suite, do the following:

  * Copy the files to `TestData\TestSuite`
  * Set the properties for the files in Visual Studio
  * Modify the unit tests' class files

Implementors are encouraged to submit their output files to the test suite repository. To generate the output files, set the constant `SaveOutput` in the class `DataCategoryTestSuiteTests` to `true`, and to specify the output directory, modify the `ActualOutputFilename` constant, and run the tests. By default, `ActualOutputFilename` points to a directory in the project. If you are using TFS with Visual Studio, don't foget to `Check Out for Edit` the directory before running the tests.

Other tests
-----------

There are some other tests in the `Tests` directory. They may use external files (so that it's easier to share the test files with others) which are located in the `TestData` directory. These files should have the same properties as the Test Suite files described above.
