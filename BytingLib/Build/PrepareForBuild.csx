using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;


string projectName = Args[0];
string reference = "";
if (Args.Count > 1)
    reference = Args[1];
string[] references = reference.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);

string contentDir = Path.Combine(Environment.CurrentDirectory, projectName, "Content");

string mgcbPath = Path.Combine(contentDir, "ContentGenerated_do-not-edit.mgcb");
string contentListPath = Path.Combine(contentDir, "ContentListGenerated_do-not-edit.txt");

WaitUntilExportingModelsFinished();

string[] pngs = GetFiles("Textures", "png");
string[] soundsWav = GetFiles("Sounds", "wav");
string[] soundsOgg = GetFiles("Sounds", "ogg");
string[] soundsMp3 = GetFiles("Sounds", "mp3");
string[] songs = GetFiles("Music", "ogg");
string[] fonts = GetFiles("Fonts", "spritefont");
string[] effects = GetFiles("Effects", "fx");
string[] models = GetFiles("Models", "fbx");

List<string> copiesList = new List<string>();
copiesList.AddRange(GetFiles("Fonts", "xnb"));
copiesList.AddRange(GetFiles("Textures", "ani")); // animations
AddIfExists(copiesList, Path.Combine("Sounds", "settings.txt"));
AddIfExists(copiesList, "localization.csv");

string[] copies = copiesList.ToArray();

string[] GetFiles(string name, string extension, SearchOption searchOption = SearchOption.AllDirectories)
{
    string dir = Path.Combine(contentDir, name);
    if (!Directory.Exists(dir))
        return new string[0];
    return Directory.GetFiles(dir, "*." + extension, SearchOption.AllDirectories);
}

void AddIfExists(List<string> list, string file)
{
    file = Path.Combine(contentDir, file);
    if (File.Exists(file))
        list.Add(file);
}


string[][] filesArray = new string[][]{
    pngs, soundsWav, soundsOgg, soundsMp3, songs, fonts, effects, models, copies
};


List<string> files = new List<string>();

for (int i = 0; i < filesArray.Length; i++)
{
    TrimPaths(filesArray[i]);
    files.AddRange(filesArray[i]);
}

void TrimPaths(string[] paths)
{
    for (int i = 0; i < paths.Length; i++)
    {
        paths[i] = paths[i].Substring(contentDir.Length + 1);
        paths[i] = paths[i].Replace("\\", "/");
    }
}


using (FileStream fs = File.Create(contentListPath))
using (StreamWriter sw = new StreamWriter(fs))
{
    for (int i = 0; i < files.Count; i++)
    {
        sw.WriteLine(files[i]);
    }
}

using (FileStream fs = File.Create(mgcbPath))
using (StreamWriter sw = new StreamWriter(fs))
{
    sw.WriteLine(@"
#----------------------------- Global Properties ----------------------------#

/outputDir:bin/$(Platform)
/intermediateDir:obj/$(Platform)
/platform:DesktopGL
/config:
/profile:Reach
/compress:False
");

    if (references.Length > 0)
    {
        sw.WriteLine("#-------------------------------- References --------------------------------#");
        for (int i = 0; i < references.Length; i++)
        {
            sw.WriteLine("/reference:" + references[i]);
        }
    }
    sw.WriteLine("#---------------------------------- Content ---------------------------------#");

    foreach (var png in pngs)
    {
        sw.WriteLine($@"
#begin {png}
/importer:TextureImporter
/processor:TextureProcessor
/processorParam:ColorKeyEnabled=False
/processorParam:GenerateMipmaps=False
/processorParam:PremultiplyAlpha=True
/processorParam:ResizeToPowerOfTwo=False
/processorParam:MakeSquare=False
/processorParam:TextureFormat=Color
/build:{png}
");
    }

    foreach (var wav in soundsWav)
    {
        sw.WriteLine($@"
#begin {wav}
/importer:WavImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:{wav}
");
    }

    foreach (var ogg in soundsOgg)
    {
        sw.WriteLine($@"
#begin {ogg}
/importer:OggImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:{ogg}
");
    }

    foreach (var ogg in soundsMp3)
    {
        sw.WriteLine($@"
#begin {ogg}
/importer:Mp3Importer
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:{ogg}
");
    }

    foreach (var song in songs)
    {
        sw.WriteLine($@"
#begin {song}
/importer:OggImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:{song}

");
    }
    foreach (var font in fonts)
    {
        sw.WriteLine($@"
#begin {font}
/importer:FontDescriptionImporter
/processor:FontDescriptionProcessor
/processorParam:PremultiplyAlpha=True
/processorParam:TextureFormat=Compressed
/build:{font}
");
    }


    foreach (var effect in effects)
    {
        sw.WriteLine($@"
#begin {effect}
/importer:EffectImporter
/processor:EffectProcessor
/processorParam:DebugMode=Auto
/build:{effect}
");
    }

    foreach (var model in models)
    {
        sw.WriteLine($@"
#begin {model}
/importer:FbxImporter
/processor:MyModelProcessor
/build:{model}
");
    }

    foreach (var copy in copies)
    {
        sw.WriteLine($@"
#begin {copy}
/copy:{copy}
");
    }
}

void WaitUntilExportingModelsFinished()
{
    string modelsPath = Path.Combine(contentDir, "Models");

    if (!Directory.Exists(modelsPath))
        return;

    string currentlyExporting;
    while ((currentlyExporting = Directory.EnumerateFiles(modelsPath, "*.exporting", SearchOption.AllDirectories).FirstOrDefault())
        != null)
    {
        do
        {
            Console.WriteLine("waiting for *.exporting files to disappear...");
            Thread.Sleep(500);
        } while (File.Exists(currentlyExporting));
    }
}

// TODO: delete generated content file after release?