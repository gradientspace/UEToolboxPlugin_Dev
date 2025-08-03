
REM nuke existing build
rmdir /s /q SOURCE_DISTRIB

set BUILDDIR=%cd%\SOURCE_DISTRIB\GradientspaceUEToolbox
rmdir /s /q %BUILDDIR%
mkdir %BUILDDIR%

set PLUGINDIR=..\Plugins\GradientspaceUEToolbox

copy %PLUGINDIR%\GradientspaceUEToolbox.uplugin %BUILDDIR%\GradientspaceUEToolbox.uplugin
xcopy %PLUGINDIR%\Config\ %BUILDDIR%\Config\ /E /R
xcopy %PLUGINDIR%\Content\ %BUILDDIR%\Content\ /E /R
xcopy %PLUGINDIR%\Source\ %BUILDDIR%\Source\ /E /R

copy FAB_LICENSE.txt %BUILDDIR%

REM make sure packager is built
call "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" FabPackager/FabPackager.sln /Build "Release"

REM run packager to clean up pdbs, uplugin
call "%cd%\FabPackager\release\FabPackager.exe" %BUILDDIR%




