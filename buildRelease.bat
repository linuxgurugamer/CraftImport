@echo off
set DEFHOMEDRIVE=d:
set DEFHOMEDIR=%DEFHOMEDRIVE%%HOMEPATH%
set HOMEDIR=
set HOMEDRIVE=%CD:~0,2%

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"
echo Default homedir: %DEFHOMEDIR%

rem set /p HOMEDIR= "Enter Home directory, or <CR> for default: "

if "%HOMEDIR%" == "" (
set HOMEDIR=%DEFHOMEDIR%
)
echo %HOMEDIR%

SET _test=%HOMEDIR:~1,1%
if "%_test%" == ":" (
set HOMEDRIVE=%HOMEDIR:~0,2%
)



set VERSIONFILE=craftimport.version
rem The following requires the JQ program, available here: https://stedolan.github.io/jq/download/
c:\local\jq-win64  ".VERSION.MAJOR" %VERSIONFILE% >tmpfile
set /P major=<tmpfile

c:\local\jq-win64  ".VERSION.MINOR"  %VERSIONFILE% >tmpfile
set /P minor=<tmpfile

c:\local\jq-win64  ".VERSION.PATCH"  %VERSIONFILE% >tmpfile
set /P patch=<tmpfile

c:\local\jq-win64  ".VERSION.BUILD"  %VERSIONFILE% >tmpfile
set /P build=<tmpfile
del tmpfile
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%

type craftimport.version

echo Version:  %VERSION%

set /p VERSION= "Enter version: "



set d=%HOMEDIR\install
if exist %d% goto one
mkdir %d%
:one
set d=%HOMEDIR%\install\Gamedata
if exist %d% goto two
mkdir %d%
:two
set d=%HOMEDIR%\install\Gamedata\CraftImport
if exist %d% goto three
mkdir %d%
:three
set d=%HOMEDIR%\install\Gamedata\CraftImport\Plugins
if exist %d% goto four
mkdir %d%
:four
set d=%HOMEDIR%\install\Gamedata\CraftImport\Textures
if exist %d% goto five
mkdir %d%
:five
del %HOMEDIR%\install\Gamedata\CraftImport\Textures\*.*

rem xcopy src\Textures\CI*.* %HOMEDIR%\install\Gamedata\CraftImport\Textures /y
xcopy src\Textures\CI-*.png   %HOMEDIR%\install\GameData\CraftImport\Textures /Y
xcopy src\Textures\colorpicker_texture.jpg %HOMEDIR%\install\GameData\CraftImport\Textures /Y
copy bin\ReleaseExport\CraftImport.dll %HOMEDIR%\install\Gamedata\CraftImport\Plugins
copy craftimport.version %HOMEDIR%\install\Gamedata\CraftImport
copy ..\MiniAVC.dll %HOMEDIR%\install\Gamedata\CraftImport
copy README.md %HOMEDIR%\install\Gamedata\CraftImport
copy ChangeLog.txt %HOMEDIR%\install\Gamedata\CraftImport
pause

%HOMEDRIVE%
cd %HOMEDIR%\install

set FILE="%RELEASEDIR%\CraftImport-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% Gamedata\CraftImport

rem %ZIP% a -tzip %RELEASEDIR%\CraftImport-%VERSION%.zip Gamedata\CraftImport
