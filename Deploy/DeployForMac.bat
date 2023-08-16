REM v1.1
REM v1.2 OSTarget=OSX

pushd .

@RD /S /Q "Release/Mac/"

REM Get DeployName and ProjectName
set /p DeployName=<DeployName.txt

cd ..
for %%I in (.) do set CurrDirName=%%~nxI

if "%DeployName%" == "" (
	set DeployName=%CurrDirName%
)


cd %CurrDirName%
dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false /p:OSTarget=OSX --self-contained true -o "../Deploy/Release/Mac/%DeployName%.app/Contents/MacOS/"

cd "../Deploy"
REM copy Info.plist
xcopy "MacFiles\Info.plist" "Release\Mac\%DeployName%.app\Contents\"
REM create directory Resources
md "Release\Mac\%DeployName%.app\Contents\Resources"
REM copy friend.icns
xcopy "MacFiles\friend.icns" "Release\Mac\%DeployName%.app\Contents\Resources"
REM copy Content dir (if exists)
REM xcopy "Release\Mac\%DeployName%.app\Contents\MacOS\Content" "Release\Mac\%DeployName%.app\Contents\Resources\Content" /s /e
move "Release\Mac\%DeployName%.app\Contents\MacOS\Content" "Release\Mac\%DeployName%.app\Contents\Resources"


popd
pushd .

REM get version from %DeployName%.dll
cd Release/Mac
FOR /F "USEBACKQ" %%F IN (`powershell -NoLogo -NoProfile -Command ^(Get-Item '%DeployName%.app/Contents/MacOS/%DeployName%.dll'^).VersionInfo.FileVersion`) DO (SET Version=%%F)
echo File version: %Version%

REM rename folder
set OldName=%DeployName%
set DeployName=%OldName%_%Version%_Mac
mkdir "%DeployName%"
move "%OldName%.app" "%DeployName%/%OldName%.app"

popd
pushd .



REM I'm using 7z, as tar doesn't keep the execution rights
cd ../../BytingLib/7z
7za.exe a -ttar -so -an "../../%CurrDirName%/Deploy/Release/Mac/%DeployName%" | 7za.exe a -si "../../%CurrDirName%/Deploy/Release/Mac/%DeployName%.tar.gz"

popd
