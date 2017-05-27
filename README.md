# PowerManagerAPI
A managed wrapper for the most important functions in the Windows Power Management API (powrprof.dll)

## Getting Started
### Installing the package
You can download the package from NuGet using the following command in the Package Manager Console:
```
PM> Install-Package PowerManagerAPI -Pre
```
If you prefer to use the NuGet packege manager in Visual Studio, __check the box to include prerelease versions__
before you search for the package 
### Writing code
Remember to add the PowerManagerAPI namespace:
```
using PowerManagerAPI;
```
Then, you can start using the static `PowerManager` function to interact with the API. For example, to get 
the id for the active plan, and then display it's name you can use:
```
var activePlanGuid = PowerManager.GetActivePlan();
var name = PowerManager.GetPlanName(activePlanGuid);
```
As long as you remember the `PoweManager` part, intellisense will probably be enough to figure out the rest, 
but if you get stuck, have a look in the `PowerManagerAPI.Tests` project for some more examples.

## Getting Help
If you have any question, please contact me through my website at [binarypoetry.co](http://binarypoetry.co).

I'll be happy to answer any questions I can. I am also available for freelance work.

## Included Functions

The following API functions are implemented in the wrapper.

- PowerEnumerate
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