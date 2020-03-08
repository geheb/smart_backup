using SimpleExec;
using System;
using static Bullseye.Targets;
using static SimpleExec.Command;
using static Geheb.SmartBackup.Build.PathExtensions;
using static Geheb.SmartBackup.Build.CommandLineExtensions;
using System.IO.Compression;
using System.IO;

namespace Geheb.SmartBackup.Build
{
    class Program
    {
        const string SolutionFile = "./SmartBackup.sln";
        const string ArtifactsPath = "./artifacts";
        const string SmartBackupOutputPath = "./artifacts/app";
        const string SmartBackupProject = "./src/SmartBackup.csproj";
        const string Configuration = "Release";
        const string Runtime = "win10-x64";

        static void Main(string[] args)
        {
            Target("restore", () =>
            {
                Console.Out.WriteLine("Restore nugets ...");
                Run("dotnet", $"restore {ExpandPath(SolutionFile)} --runtime {Runtime} --verbosity minimal");
            });

            Target("clean-artifacts", () =>
            {
                Console.Out.WriteLine("Clean artifacts ...");
                CleanDirectory(ExpandPath(ArtifactsPath));
            });

            Target("clean-solution", DependsOn("restore"), () =>
            {
                Console.Out.WriteLine("Clean solution ...");
                Run("dotnet", $"clean {ExpandPath(SolutionFile)} --runtime {Runtime} --configuration {Configuration} --nologo --verbosity minimal");
            });

            Target("build", DependsOn("clean-solution"), () =>
            {
                Console.Out.WriteLine("Build solution ...");
                Run("dotnet", $"build {ExpandPath(SolutionFile)} --runtime {Runtime} --configuration {Configuration} --nologo --verbosity minimal --no-restore");
            });

            Target("copy-7z", () =>
            {
                Console.Out.WriteLine("Copy 7z runtime ...");
                CopyAll(ExpandPath("./packages/7-zip.standaloneconsole/19.0.0/tools"), ExpandPath(SmartBackupOutputPath + "/7zip"));
            });

            Target("copy-appsettings", () =>
            {
                Console.Out.WriteLine("Copy appsettings  ...");
                File.Copy(ExpandPath("./src/appsettings.json"), ExpandPath(SmartBackupOutputPath + "/appsettings.json"));
            });

            Target("copy-licenses", () =>
            {
                Console.Out.WriteLine("Copy licenses  ...");
                File.Copy(ExpandPath("LICENSE"), ExpandPath(SmartBackupOutputPath + "/LICENSE"));
                CopyAll(ExpandPath("./licenses"), ExpandPath(SmartBackupOutputPath + "/licenses"));
            });

            Target("create-zip", DependsOn("copy-7z", "copy-appsettings", "copy-licenses"), () =>
            {
                Console.Out.WriteLine("Create zip release package ...");
                ZipFile.CreateFromDirectory(ExpandPath(SmartBackupOutputPath), ExpandPath($"{ArtifactsPath}/SmartBackup-{GetSemVer(args)}.zip"));
            });

            Target("publish", DependsOn("clean-artifacts", "clean-solution"), () =>
            {
                Console.Out.WriteLine("Publish app ...");
                Run("dotnet", $"publish {ExpandPath(SmartBackupProject)} --runtime {Runtime} --configuration {Configuration} --nologo --verbosity minimal --no-restore /p:PublishSingleFile=true" +
                    $" -p:Version={GetSemVer(args)} /p:InformationalVersion={GetSemVerWithGitHash(args)}" +
                    $" --output {ExpandPath(SmartBackupOutputPath)}");
            });

            Target("default", DependsOn("build"));
            Target("release", DependsOn("publish", "create-zip"));

            RunTargetsAndExit(CleanupArgs(args), messageOnly: ex => ex is NonZeroExitCodeException);
        }
    }
}
