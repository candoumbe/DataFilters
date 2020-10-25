using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;

using System.Collections.Generic;
using System.Linq;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[AzurePipelines(
    AzurePipelinesImage.UbuntuLatest,
    AzurePipelinesImage.WindowsLatest,
    InvokedTargets = new[] { nameof(Test) },
    ExcludedTargets = new[] { nameof(Clean) },
    PullRequestsAutoCancel = true,
    PullRequestsBranchesInclude = new[] { "main" },
    TriggerBranchesInclude = new[] {
        "main",
        "feature/*",
        "fix/*",
        "exp"
    },
    TriggerPathsExclude = new[]
    {
        "docs/*",
        "README.md"
    }
    )]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    [CI] readonly AzurePipelines AzurePipelines;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "test";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    [Partition(10)] readonly Partition TestPartition;

    [PathExecutable("pwsh")] readonly Tool Powershell;
    [PathExecutable("wget")] readonly Tool Wget;

    Target Clean => _ => _
        .OnlyWhenStatic(() => !IsServerBuild)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });


    //Target SetupNuget => _ => _
    //    .Requires(() => IsServerBuild)
    //    .Executes(() =>
    //    {
    //        Info("Installing Azure Credentials Provider");
    //        if (IsWin)
    //        {
    //            Powershell(arguments: @"iex "" & { $(irm https://aka.ms/install-artifacts-credprovider.ps1) }""",
    //                       logInvocation : true,
    //                       logOutput : true
    //                );
    //        }
    //        else
    //        {
    //            Wget(arguments : "-qO- https://aka.ms/install-artifacts-credprovider.sh | bash",
    //                 logInvocation: true,
    //                 logOutput: true);
    //        }
    //        Info("Azure Credentials Provider installed successfully");
    //    });

    //Target Restore => _ => _
    //    .DependsOn(Clean)
    //    .Executes(() =>
    //    {
    //        AbsolutePath configFile = RootDirectory / "Nuget.config";
    //        Info("Restoring packages");
    //        Info($"Config file : '{configFile}'");

    //        DotNetRestore(s => s
    //            .SetConfigFile(configFile)
    //            .SetIgnoreFailedSources(true)
    //            .SetProjectFile(Solution));
    //    });

    Target Compile => _ => _
        .DependsOn(Clean)
        .Executes(() =>
            DotNetBuild(s => s
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .EnableLogOutput()
                .EnableLogInvocation()
            )
        );

    Target Test => _ => _
        .DependsOn(Compile)
        .Partition(() => TestPartition)
        .Executes(() =>
        {
            IEnumerable<Project> projects = Solution.GetProjects("*Tests.csproj");
            IEnumerable<Project> currentProjects = TestPartition.GetCurrent(projects);

            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .SetVerbosity(DotNetVerbosity.Minimal)
                .EnableCollectCoverage()
                .EnableNoBuild()
                .SetCoverletOutput(TemporaryDirectory)
                .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                .AddProperty("MergeWith", TemporaryDirectory / "coverage.json")
                .AddProperty("ExcludeByAttribute", "Obsolete")
                .CombineWith(currentProjects, (cs, project) => cs.SetProjectFile(project)));
        });
}
