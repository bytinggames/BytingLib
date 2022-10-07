make automatic content building work:
1. copy PrepareForBuild.ps1 to your project directory
2. add the following lines to that projects .csproj file:
"
  <!-- added next three tags for automatic content generation -->
  <Target Name="PrepareForBuildCustom" BeforeTargets="PrepareForBuild">
    <Exec Command="Powershell.exe -executionpolicy remotesigned -File PrepareForBuild.ps1 &quot;$(DevEnvDir)&quot;" />
  </Target>
  <ItemGroup>
    <EmbeddedResource Include="Content/ContentListGenerated_do-not-edit.txt" />
  </ItemGroup>
  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="echo D|xcopy /y &quot;$(ProjectDir)Content/*.mgcb&quot; &quot;$(ProjectDir)$(OutDir)Content/*.mgcbcopy&quot;" />
  </Target>
"
3. old Content.mgcb can be deleted. But it can also persist if you want to build custom data from directories other than Fonts, Music, Sounds and Textures
4. copy the .gitignore from this folder to your Content directory. I recommend adding it to your visual studio directory directly.