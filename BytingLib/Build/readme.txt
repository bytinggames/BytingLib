Copy the "_GenerateContentFiles.tt.remove" file
edit it by inserting your csproj file name at [TODO: set project file here]
make sure your .csproj file contains a reference to BytingLib with forward slashes (/)
Remove the ".remove" from that filename.
save the _GenerateContentFiles.tt file with visual studio to trigger generating the Content.Generated.mgcb and ContentLoader.Generated.cs file in the Content directory.
Always do that after you changed the content tree.
you can delete the Content.mgcb now. Or you keep it to load custom stuff.

also: add this to the csproj:

	<!--Move all *.mgcb to output and rename to *.mgcbcopy-->
	<Target Name="PostBuild" AfterTargets="AfterBuild">
		<ItemGroup>
			<SourceFiles Include="$(ProjectDir)Content/*.mgcb" />
		</ItemGroup>
		<Copy SourceFiles="@(SourceFiles)" DestinationFolder="$(OutDir)Content/" />
		<ItemGroup>
			<SourceFiles2 Include="$(OutDir)Content/*.mgcb" />
		</ItemGroup>
		<Move SourceFiles="@(SourceFiles2)" DestinationFiles="@(SourceFiles2 -&gt; Replace('.mgcb', '.mgcbcopy'))" />
	</Target>
	<Target Name="PostPublish" AfterTargets="Publish">
		<ItemGroup>
			<SourceFilesPublish Include="$(ProjectDir)Content/*.mgcb" />
		</ItemGroup>
		<Copy SourceFiles="@(SourceFilesPublish)" DestinationFolder="$(PublishDir)Content/" />
		<ItemGroup>
			<SourceFilesPublish2 Include="$(PublishDir)Content/*.mgcb" />
		</ItemGroup>
		<Move SourceFiles="@(SourceFilesPublish2)" DestinationFiles="@(SourceFilesPublish2 -&gt; Replace('.mgcb', '.mgcbcopy'))" />
	</Target>


also: add
	<Import Project="../BytingLib/BytingLib/Build/Targets.targets" />
to your csproj, if you haven't already