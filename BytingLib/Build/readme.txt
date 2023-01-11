Copy the "_GenerateContentFiles.tt.remove" file
edit it by inserting your csproj file name at [TODO: set project file here]
Remove the ".remove" from that filename.
save the _GenerateContentFiles.tt file with visual studio to trigger generating the Content.Generated.mgcb and ContentLoader.Generated.cs file in the Content directory.
Always do that after you changed the content tree.