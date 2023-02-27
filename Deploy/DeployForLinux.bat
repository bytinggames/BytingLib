
@RD /S /Q "Release/Linux/"

REM Get DeployName and ProjectName
set /p DeployName=<DeployName.txt

cd ..
for %%I in (.) do set CurrDirName=%%~nxI

if "%DeployName%" == "" (
	set DeployName=%CurrDirName%
)




cd %CurrDirName%
dotnet publish -c Release -r linux-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false /p:OSTarget=Linux --self-contained -o "../Deploy/Release/Linux/%DeployName%"

REM I'm using 7z, as tar doesn't keep the execution rights
cd ../../BytingLib/7z
7za.exe a -ttar -so -an "../../%CurrDirName%/Deploy/Release/Linux/%DeployName%" | 7za.exe a -si "../../%CurrDirName%/Deploy/Release/Linux/%DeployName%.tar.gz"

cd "../../%CurrDirName%/Deploy"
