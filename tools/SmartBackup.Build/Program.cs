using SimpleExec;
using System;
using static Bullseye.Targets;
using static SimpleExec.Command;
using static Geheb.SmartBackup.Build.PathExtensions;
using static Geheb.SmartBackup.Build.CommandLineExtensions;
using System.IO.Compression;
using System.IO;
using System.Collections.Generic;

namespace Geheb.SmartBackup.Build
{
    class Program
    {
        const string SolutionFile = "./SmartBackup.sln";
        const string ArtifactsPath = "./artifacts";
        const string SmartBackupOutputPath = "./artifacts/app";
        const string SmartBackupProject = "./src/SmartBackup.csproj";
		const string Runtime = "win10-x64";
		const string Configuration = "Release";

        private static readonly string[] TestSuites = new[]
        {
            "./tests/SmartBackup.UnitTests"
        };

        static void Main(string[] args)
        {
            Target("restore", () =>
            {
                Run("dotnet", $"restore {ExpandPath(SolutionFile)} --runtime {Runtime}");
            });

            Target("clean-artifacts", () =>
            {
                CleanDirectory(ExpandPath(ArtifactsPath));
            });

            Target("build", DependsOn("restore"),() =>
            {
                Run("dotnet", $"build {ExpandPath(SolutionFile)} --nologo --no-restore --configuration {Configuration}");
            });

            Target("test", DependsOn("build"), () =>
            {
                foreach (var testDir in TestSuites)
                {
                    Run("dotnet", $"test --no-build --nologo --no-restore --configuration {Configuration}", ExpandPath(testDir));
                }
            });

            Target("copy-7z", () =>
            {
                CopyAll(ExpandPath("./packages/7-zip.standaloneconsole/19.0.0/tools"), ExpandPath(SmartBackupOutputPath + "/7zip"));
            });

            Target("copy-licenses", () =>
            {
                File.Copy(ExpandPath("LICENSE"), ExpandPath(SmartBackupOutputPath + "/LICENSE"));
                CopyAll(ExpandPath("./licenses"), ExpandPath(SmartBackupOutputPath + "/licenses"));
            });

            Target("create-zip", DependsOn("copy-7z", "copy-licenses"), () =>
            {
                ZipFile.CreateFromDirectory(ExpandPath(SmartBackupOutputPath), ExpandPath($"{ArtifactsPath}/SmartBackup-{GetSemVer(args)}.zip"));
            });

            Target("publish", DependsOn("clean-artifacts"), () =>
            {
                Run("dotnet", $"publish {ExpandPath(SmartBackupProject)} --runtime {Runtime} --configuration {Configuration} --nologo --no-restore" +
                    " -p:PublishTrimmed=true -p:PublishReadyToRun=true" +
                    $" -p:Version={GetSemVerFull(args)}" +
                    $" --output {ExpandPath(SmartBackupOutputPath)}");
            });

            Target("default", DependsOn("build"));
            Target("release", DependsOn("test", "publish", "create-zip"));

            RunTargetsAndExit(CleanupArgs(args), messageOnly: ex => ex is NonZeroExitCodeException);
        }
    }
}
