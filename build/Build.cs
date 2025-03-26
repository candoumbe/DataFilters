using Nuke.Common.Tools.GitHub;

namespace DataFilters.ContinuousIntegration;

using System;
using System.Collections.Generic;
using System.Linq;
using Candoumbe.Pipelines.Components;
using Candoumbe.Pipelines.Components.Formatting;
using Candoumbe.Pipelines.Components.GitHub;
using Candoumbe.Pipelines.Components.NuGet;
using Candoumbe.Pipelines.Components.Workflows;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

[GitHubActions(
    "integration",
    GitHubActionsImage.Ubuntu2204,
    AutoGenerate = false,
    FetchDepth = 0,
    OnPushBranchesIgnore = [IHaveMainBranch.MainBranchName],
    PublishArtifacts = true,
    InvokedTargets = [nameof(IUnitTest.UnitTests), nameof(IPushNugetPackages.Publish), nameof(IPack.Pack)],
    CacheKeyFiles = ["global.json", "src/**/*.csproj"],
    ImportSecrets =
    [
        nameof(NugetApiKey),
        nameof(IReportCoverage.CodecovToken),
    ],
    OnPullRequestExcludePaths =
    [
        "docs/*",
        "README.md",
        "CHANGELOG.md",
        "LICENSE"
    ]
)]
[GitHubActions(
    "delivery",
    GitHubActionsImage.Ubuntu2204,
    AutoGenerate = false,
    FetchDepth = 0,
    OnPushBranches = [IHaveMainBranch.MainBranchName, IGitFlow.ReleaseBranch + "/*"],
    InvokedTargets = [nameof(IUnitTest.UnitTests), nameof(IPushNugetPackages.Publish), nameof(ICreateGithubRelease.AddGithubRelease)],
    EnableGitHubToken = true,
    CacheKeyFiles = ["global.json", "src/**/*.csproj"],
    PublishArtifacts = true,
    ImportSecrets =
    [
        nameof(NugetApiKey),
        nameof(IReportCoverage.CodecovToken)
    ],
    OnPullRequestExcludePaths =
    [
        "docs/*",
        "README.md",
        "CHANGELOG.md",
        "LICENSE"
    ]
)]

//[GitHubActions("nightly", GitHubActionsImage.Ubuntu2204,
//    AutoGenerate = false,
//    FetchDepth = 0,
//    OnCronSchedule = "0 0 * * *",
//    InvokedTargets = new[] { nameof(IUnitTest.Compile), nameof(IMutationTest.MutationTests), nameof(IPushNugetPackages.Pack) },
//    OnPushBranches = new[] { IHaveDevelopBranch.DevelopBranchName },
//    CacheKeyFiles = new[] {
//        "src/**/*.csproj",
//        "test/**/*.csproj",
//        "stryker-config.json",
//        "test/**/*/xunit.runner.json" },
//    EnableGitHubToken = true,
//    ImportSecrets = new[]
//    {
//        nameof(NugetApiKey),
//        nameof(IReportCoverage.CodecovToken),
//        nameof(IMutationTest.StrykerDashboardApiKey)
//    },
//    PublishArtifacts = true,
//    OnPullRequestExcludePaths = new[]
//    {
//        "docs/*",
//        "README.md",
//        "CHANGELOG.md",
//        "LICENSE"
//    }
//)]
[GitHubActions("nightly-manual", GitHubActionsImage.Ubuntu2204,
    AutoGenerate = false,
    FetchDepth = 0,
    On = [GitHubActionsTrigger.WorkflowDispatch],
    InvokedTargets = [nameof(IUnitTest.Compile), nameof(IMutationTest.MutationTests), nameof(IPushNugetPackages.Pack)],
    CacheKeyFiles = [
        "src/**/*.csproj",
        "test/**/*.csproj",
        "stryker-config.json",
        "test/**/*/xunit.runner.json"],
    EnableGitHubToken = true,
    ImportSecrets =
    [
        nameof(NugetApiKey),
        nameof(IReportCoverage.CodecovToken),
        nameof(IMutationTest.StrykerDashboardApiKey)
    ],
    PublishArtifacts = true
)]
[DotNetVerbosityMapping]
public class Build : EnhancedNukeBuild,
    IHaveSolution,
    IHaveSourceDirectory,
    IHaveTestDirectory,
    IGitFlowWithPullRequest,
    IClean,
    IRestore,
    ICompile,
    IUnitTest,
    IMutationTest,
    IBenchmark,
    IReportCoverage,
    IPack,
    IPushNugetPackages,
    ICreateGithubRelease
{
    [Parameter("API key used to publish artifacts to Nuget.org")]
    [Secret]
    public readonly string NugetApiKey;

    [Solution]
    [Required]
    public readonly Solution Solution;

    ///<inheritdoc/>
    Solution IHaveSolution.Solution => Solution;

    public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IClean.DirectoriesToDelete
        =>
        [
            .. this.Get<IHaveSourceDirectory>().SourceDirectory.GlobDirectories("**/bin", "**/obj"),
            .. this.Get<IHaveTestDirectory>().TestDirectory.GlobDirectories("**/bin", "**/obj")
        ];

    /// <inheritdoc />
    Configure<DotNetToolRestoreSettings> IRestore.RestoreToolSettings => _ => _.SetDisableParallel(true);

    ///<inheritdoc/>
    AbsolutePath IHaveSourceDirectory.SourceDirectory => RootDirectory / "src";

    ///<inheritdoc/>
    AbsolutePath IHaveTestDirectory.TestDirectory => RootDirectory / "test";

    ///<inheritdoc/>
    IEnumerable<Project> IUnitTest.UnitTestsProjects => this.Get<IHaveSolution>().Solution.GetAllProjects("*UnitTests");

    private static IReadOnlyList<string> Projects => ["DataFilters", "DataFilters.Expressions", "DataFilters.Queries"];

    ///<inheritdoc/>
    IEnumerable<MutationProjectConfiguration> IMutationTest.MutationTestsProjects
        => Projects.Select(projectName => new MutationProjectConfiguration(sourceProject: Solution.AllProjects.Single(csproj => string.Equals(csproj.Name, projectName, StringComparison.InvariantCultureIgnoreCase)),
                testProjects: Solution.AllProjects.Where(csproj => string.Equals(csproj.Name, $"{projectName}.UnitTests", StringComparison.InvariantCultureIgnoreCase)),
                configurationFile: this.Get<IHaveTestDirectory>().TestDirectory / $"{projectName}.UnitTests" / "stryker-config.json"))
            .ToArray();

    ///<inheritdoc/>
    IEnumerable<Project> IBenchmark.BenchmarkProjects => this.Get<IHaveSolution>().Solution.GetAllProjects("*.PerformanceTests");

    ///<inheritdoc/>
    bool IReportCoverage.ReportToCodeCov => this.Get<IReportCoverage>().CodecovToken is not null;

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IPack.PackableProjects => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");

    ///<inheritdoc/>
    IEnumerable<PushNugetPackageConfiguration> IPushNugetPackages.PublishConfigurations =>
    [
        new NugetPushConfiguration(apiKey: NugetApiKey,
            source: new Uri("https://api.nuget.org/v3/index.json"),
            canBeUsed: () => NugetApiKey is not null),
        new GitHubPushNugetConfiguration(githubToken: this.Get<IHaveGitHubRepository>().GitHubToken,
            source: new Uri($"https://nuget.pkg.github.com/{this.Get<IHaveGitHubRepository>().GitRepository.GetGitHubOwner()}/index.json"),
            canBeUsed: () => this.As<ICreateGithubRelease>()?.GitHubToken is not null)
    ];

    protected override void OnBuildCreated()
    {
        if (IsServerBuild)
        {
            EnvironmentInfo.SetVariable("DOTNET_ROLL_FORWARD", "LatestMajor");
        }
    }
}