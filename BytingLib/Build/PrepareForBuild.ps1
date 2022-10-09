# v1.3.1

# get to solution level
cd ..;

$csi = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csi.exe";

$projects = Get-Content '*.sln' |
  Select-String 'Project\(' |
    ForEach-Object {
      $projectParts = $_ -Split '[,=]' | ForEach-Object { $_.Trim('[ "{}]') };
      New-Object PSObject -Property @{
        Name = $projectParts[1];
        File = $projectParts[2];
        Guid = $projectParts[3]
      }
    }


$index = $projects.Name.IndexOf("BytingLib");
$path = $projects.File[$index];
$path = (get-item $path).Directory.FullName;
$path += "\Build\PrepareForBuild.csx";
# echo "csx path: $path";

$projectName = (get-item $PSScriptRoot).Name;
# echo "project name: $projectName";

echo "Executing: '$csi' '$path' $projectName $args";
. "$csi" "$path" $projectName $args;