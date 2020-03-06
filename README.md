# Overview

This package provides a build-time check that the set of referenced assemblies and the versions of those references have not changed from the previous build.

This helps avoid accidental changes that can cause failures at runtime e.g.
* by referencing a newer version of an assembly that is not supported by the minimum framework version targeted by the application
* by adding a reference to an assembly that is not packaged with the application

## Instructions for use:

### Initial setup
1. Add the NuGet package `Devtility.CheckAsmRefs`
2. Build the project.
   The first time you build your project after adding a reference to this package, a baseline file will be created.
3. Add the baseline file to source control.

### Day to day usage
Continue to work on your app as normal. On every subsequent build, a new assembly references report will be created and compared against the baseline file. The build will fail if there are differences. At that point:

* compare the baseline file against the current report file.
* If the changes are unintentional, undo them.
* If the changes are intentional, check that you have made any other relevant changes (such as updating your install package to include any newly-referenced assemblies).
Then, update the baseline file and check in the new baseline.

The baseline file can be updated by re-running the build with the following property: `/p:AsmRefUpdateBaseline=true`
