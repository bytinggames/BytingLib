call ".\DeployForWindowsNoZip.bat"

pushd .

cd Release/Windows

tar -caf "%DeployName%.zip" "%DeployName%"

popd