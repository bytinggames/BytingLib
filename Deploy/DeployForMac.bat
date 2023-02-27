REM v1.1
REM v1.2 OSTarget=OSX

@RD /S /Q "Release/Mac/"

REM Get DeployName and ProjectName
set /p DeployName=<DeployName.txt

cd ..
for %%I in (.) do set CurrDirName=%%~nxI

if "%DeployName%" == "" (
	set DeployName=%CurrDirName%
)


cd %CurrDirName%
dotnet publish -c Release -r osx-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false /p:OSTarget=OSX --self-contained -o "../Deploy/Release/Mac/%DeployName%.app/Contents/MacOS/"

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

REM I'm using 7z, as tar doesn't keep the execution rights
cd ../../BytingLib/7z
7za.exe a -ttar -so -an "../../%CurrDirName%/Deploy/Release/Mac/%DeployName%.app" | 7za.exe a -si "../../%CurrDirName%/Deploy/Release/Mac/%DeployName%.tar.gz"

cd "../../%CurrDirName%/Deploy"
