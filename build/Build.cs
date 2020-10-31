using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotCover;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Tools.ReSharper;
using Nuke.Common.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using static Nuke.Common.Logger;
using System.ComponentModel;
using Nuke.Common.Tools.NuGet;

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
    },
    ImportVariableGroups = new[] { "Tokens" }
    )]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    [CI] readonly AzurePipelines AzurePipelines;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "test";

    AbsolutePath OutputDirectory = RootDirectory / "output";
    AbsolutePath ArtifactsDirectory => OutputDirectory / "artifacts";

    [Partition(10)] readonly Partition TestPartition;

    [Parameter("Token to access private feeds")] readonly string NugetToken;

    Target Clean => _ => _
        .OnlyWhenStatic(() => !IsServerBuild)
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            AbsolutePath configFile = RootDirectory / "Nuget.config";
            Info("Restoring packages");
            Info($"Config file : '{configFile}'");


            IEnumerable<Project> projects = Solution.AllProjects;

            var envValue = new
            {
                endpointCredentials = new[]
                {
                    new
                    {
                        endpoint = "https://pkgs.dev.azure.com/candoumbe/_packaging/Filters/nuget/v3/index.json",
                        username = "ndoumbecyrille@hotmail.com",
                        pasword = NugetToken
                    }
                }
            };
            Environment.SetEnvironmentVariable("VS_NUGET_EXTERNAL_FEED_ENDPOINTS", envValue.Jsonify());

            DotNetRestore(s => s
                .SetConfigFile(configFile)
                .ClearSources()
                .AddSources("https://pkgs.dev.azure.com/candoumbe/_packaging/Filters/nuget/v3/index.json")
                .SetIgnoreFailedSources(true)
                .SetProjectFile(Solution)
                );
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>

            DotNetBuild(s => s
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .SetProcessLogOutput(true)
                .SetProcessLogInvocation(true)
                .SetNoRestore(InvokedTargets.Contains(Restore))
            )
        );

    /// <summary>
    /// Path to the directory that contains all tests results;
    /// </summary>
    AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

    [DisplayName("Run tests")]
    Target Test => _ => _
        .DependsOn(Compile)
        .Partition(() => TestPartition)
        .Produces(TestResultDirectory / "*.xml")
        .Produces(TestResultDirectory / "*.trx")
        .Executes(() =>
        {
            IEnumerable<Project> projects = Solution.GetProjects("*Tests");
            IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .ResetVerbosity()
                .EnableCollectCoverage()
                .SetNoBuild(InvokedTargets.Contains(Compile))
                .SetResultsDirectory(TestResultDirectory)
                .When(IsServerBuild, _ => _
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .AddProperty("ExcludeByAttribute", "Obsolete")
                    .EnableUseSourceLink()
                )
                .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                    .SetLogger($"trx;LogFileName={project.Name}.trx")
                    .When(InvokedTargets.Contains(Coverage) || IsServerBuild, _ => _
                        .SetCoverletOutput(TestResultDirectory / $"{project.Name}.xml"))));

            TestResultDirectory.GlobFiles("*.trx").ForEach(testFileResult =>
                AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                   title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                   files: new string[] { testFileResult }));
        });

    AbsolutePath PackageDirectory => OutputDirectory / "packages";

    AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";
    AbsolutePath CoverageReportArchive => OutputDirectory / "coverage-report.zip";

    Target Coverage => _ => _
       .DependsOn(Test)
       .TriggeredBy(Test)
       .Consumes(Test)
       .Executes(() =>
       {
           ReportGenerator(_ => _
               .SetReports(TestResultDirectory / "*.xml")
               .SetReportTypes(ReportTypes.HtmlInline)
               .SetTargetDirectory(CoverageReportDirectory));

           IReadOnlyCollection<AbsolutePath> testResultFiles = TestResultDirectory.GlobFiles("*.xml");
           testResultFiles.ForEach(x =>
               AzurePipelines?.PublishCodeCoverage(AzurePipelinesCodeCoverageToolType.Cobertura,
                                                   x,
                                                   CoverageReportDirectory));

           CompressZip(directory: CoverageReportDirectory,
                       archiveFile: CoverageReportArchive,
                       fileMode: FileMode.Create);
       });
}
