add the following to your .csproj:

	<Target Name="PostBuild" AfterTargets="PostBuildEvent">
	  <Exec Command="echo D|xcopy /y &quot;$(ProjectDir)Content/*.mgcb&quot; &quot;$(ProjectDir)$(OutDir)Content/*.mgcbcopy&quot;" />
	</Target>
