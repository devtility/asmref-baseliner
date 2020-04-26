# Release Notes v0.10.0-rc

* Separate variables for AsmRefSourceVersionCompatibility and AsmRefTargetVersionCompatibility
(the previous variable AsmRefVersionCompatibility can still be used to set both to the same value)

* added end-to-end tests for MSBuild and dotnet on Windows

* fix bug: invalid yaml that prevented comparisons for reports containing LoadExceptions [#69]

# Release Notes v0.9.0-rc

* Added AsmRefIgnoreSourcePublicKeyToken option to ignore source assembly PublicKeyToken when comparing
Default is true i.e. PublicKeyTokens will be ignored. [#48]

# Release Notes v0.8.0-rc

* added ability to specify version compatibility to use when checking (`Any`, `Major`, `MajorMinor`, `MajorMinorBuild`, `Strict`) [#32]

# Release Notes v0.7.0-rc

* initial release - basic MSBuild workflow, no configuration of comparison options





