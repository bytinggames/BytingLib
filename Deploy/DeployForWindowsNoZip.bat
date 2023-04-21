pushd .

@RD /S /Q "Release/Windows/"

REM Get DeployName and ProjectName
set /p DeployName=<DeployName.txt

cd ..
for %%I in (.) do set CurrDirName=%%~nxI

if "%DeployName%" == "" (
	set DeployName=%CurrDirName%
)


cd "%CurrDirName%"
dotnet publish -c Release -r win-x64 /p:PublishReadyToRun=false /p:TieredCompilation=false /p:OSTarget=Windows --self-contained -o "../Deploy/Release/Windows/%DeployName%"

popd