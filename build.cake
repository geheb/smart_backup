var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var solutionFile = File("./build.sln");
var buildDir = Directory("./bin");
var buildConfigDirectory = buildDir + Directory(configuration);
var packagesDirectory = Directory("./packages");

Task("clean")
	.Does(() =>
{
	CleanDirectory(buildDir);
});

Task("build-anycpu")
	.Does(() =>
{
	MSBuild(solutionFile, settings => settings
		.SetConfiguration(configuration)
		.SetPlatformTarget(PlatformTarget.MSIL)
		.WithTarget("Rebuild")
		.SetVerbosity(Verbosity.Minimal));
});

Task("zip")
	.Does(() =>
{
	var target7zipDir = buildConfigDirectory + Directory("7-zip");
	CreateDirectory(target7zipDir);
	CopyFiles(packagesDirectory.ToString() + "/7-zip.commandline/**/tools/*", target7zipDir);
	target7zipDir += Directory("x64");
	CreateDirectory(target7zipDir);
	CopyFiles(packagesDirectory.ToString() + "/7-zip.commandline/**/tools/x64/*", target7zipDir);
	
	CopyFile("./LICENSE", buildConfigDirectory.ToString() + "/LICENSE.txt");
	CopyFiles("./LICENSE-3RD-PARTY.txt", buildConfigDirectory);
	
	var ignoredExt = new string[] { ".xml", ".pdb" };
	var files = GetFiles(buildConfigDirectory.ToString() + "/**/*")
		.Where(f => !ignoredExt.Contains(f.GetExtension().ToLowerInvariant()));
		
	Zip(buildConfigDirectory, buildDir.ToString() + "/release.zip", files);
});

Task("build")
	.IsDependentOn("clean")
	.IsDependentOn("build-anycpu")
	.IsDependentOn("zip")
	.Does(() =>
{
});

Task("Default")
    .IsDependentOn("build")
	.Does(()=> 
{ 
});
	
RunTarget(target);
