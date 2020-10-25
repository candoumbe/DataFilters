using System;
using System.Collections.Generic;
using System.Linq;

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

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Logger;

[AzurePipelines(
    AzurePipelinesImage.UbuntuLatest,
    AzurePipelinesImage.WindowsLatest,
    InvokedTargets = new[] { nameof(Test) },
    NonEntryTargets = new[] { nameof(Restore) },
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

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            AbsolutePath configFile = RootDirectory / "Nuget.config";
            Info("Restoring packages");
            Info($"Config file : '{configFile}'");

            IEnumerable<Project> projects = Solution.AllProjects
                                                    .Where(proj => !proj.Name.StartsWith("_"));

            foreach (Project item in projects)
            {
                Info($"Restoring {item} ...");

                DotNetRestore(s => s
                    .SetConfigFile(configFile)
                    .SetIgnoreFailedSources(true)
                    .SetProjectFile(item));
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            IEnumerable<Project> projects = Solution.GetProjects("*.csproj")
                                                    .Where(proj => !proj.Name.StartsWith("_"));

            DotNetBuild(s => s
                .SetConfiguration(Configuration)
                .CombineWith(projects, (setting, proj) => setting.SetProjectFile(proj)
                                                                 .EnableNoRestore())
            );
        });

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
