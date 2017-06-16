@echo off
 
rem Test command line.
if '%1'=='' goto usage
if not '%3'=='' goto usage

rem Invoke MSBuild
echo on
msbuild ExecutePostSharp.proj "/p:In=%0" "/p:Out=%1" "/p:ReferenceDirectory=%CD%" "/p:Parameters=%2"
@echo off
goto end

:usage
echo PostSharpInMSBuild.cmd {Input} {Output} [{Parameters}]
echo.
echo This target executes PostSharp on an existing assembly. This target is typically
echo invoked manually for debugging. It is not a part of the standard build process.

:end