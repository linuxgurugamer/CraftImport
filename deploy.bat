
@echo off

set H=R:\KSP_1.7.2_dev
set GAMEDIR=CraftImport

echo %H%

copy /Y "%1%2" "GameData\%GAMEDIR%\Plugins"
copy /Y %GAMEDIR%.version GameData\%GAMEDIR%

mkdir "%H%\GameData\%GAMEDIR%"
xcopy /y /s GameData\%GAMEDIR% "%H%\GameData\%GAMEDIR%"

