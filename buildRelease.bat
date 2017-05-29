
@echo off

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"

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


set d=Gamedata
if exist %d% goto two
mkdir %d%
:two
set d=Gamedata\CraftImport
if exist %d% goto three
mkdir %d%
:three
set d=Gamedata\CraftImport\Plugins
if exist %d% goto four
mkdir %d%
:four
set d=Gamedata\CraftImport\Textures
if exist %d% goto five
mkdir %d%
:five


rem xcopy src\Textures\CI*.* Gamedata\CraftImport\Textures /y
xcopy src\Textures\CI-*.png   GameData\CraftImport\Textures /Y
xcopy src\Textures\colorpicker_texture.jpg GameData\CraftImport\Textures /Y
copy bin\ReleaseExport\CraftImport.dll Gamedata\CraftImport\Plugins
copy craftimport.version Gamedata\CraftImport
copy ..\MiniAVC.dll Gamedata\CraftImport
copy README.md Gamedata\CraftImport
copy ChangeLog.txt Gamedata\CraftImport

set FILE="%RELEASEDIR%\CraftImport-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% Gamedata\CraftImport

pause