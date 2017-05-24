# PowerManagerAPI
A managed wrapper for the most important functions in the Windows Power Management API (powrprof.dll)

## Included functions

The following API functions are implemented in the wrapper.

- PowerGetActiveScheme
- PowerSetActiveScheme
- PowerDuplicateScheme
- PowerDeleteScheme
- PowerReadFriendlyName
- PowerWriteFriendlyName
- PowerReadDescription
- PowerWriteDescription
- PowerReadACValueIndex
- PowerWriteACValueIndex
- PowerReadDCValueIndex
- PowerWriteDCValueIndex

Rather than exposing these methods directly, the wrapper exposes a static class called `PowerManager` with 
a number of managed methods that will feel familiar to .Net developers. There are also lookups for Subgroup 
and Setting Guids, to help you get to the setting you want.

## Compatibility
The implemented API functions are available in Windows Vista/Windows Server 2003 and later. Earlier versions of
Windows used a different set of power management functions, but since those versions are mostly history, they 
are out of the scope for this library.

## Tests
There is a test project that covers most of the `PowerManager`. Since this is an API, the tests run against the 
actual windows API, because even if mocking it were possible it would render the tests useless.

Since the tests are running agains the actual API, they do have side effects, since power plans are created, 
edited and deleted. The tests do not alter the standard Windows plans, but create and work on their own dummy
plans. Conceivably, such a plan could in some cases be left over after running the tests. They can be deleted
through the `powercfg` command line tool or Control Panel.

## Getting started
The NuGet package is coming soon, but now you have to settle for cloning or downloading the source.

In using this library, you will mostly interact with the methods of the static `PowerManager` function. Have a
look at the tests to see some example code.