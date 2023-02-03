namespace DataFilters.ContinuousIntegration
{
    using Candoumbe.Pipelines;
    using Candoumbe.Pipelines.Components;
    using Candoumbe.Pipelines.Components.GitHub;
    using Candoumbe.Pipelines.Components.Workflows;

    using Nuke.Common;
    using Nuke.Common.CI.GitHubActions;
    using Nuke.Common.IO;
    using Nuke.Common.ProjectModel;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using static Nuke.Common.IO.PathConstruction;

    [GitHubActions(
        "integration",
        GitHubActionsImage.UbuntuLatest,
        FetchDepth = 0,
        OnPushBranchesIgnore = new[] { IHaveMainBranch.MainBranchName },
        PublishArtifacts = true,
        InvokedTargets = new[] { nameof(IUnitTest.UnitTests), nameof(IPublish.Publish), nameof(IPack.Pack) },
        CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" },
        ImportSecrets = new[]
        {
            nameof(NugetApiKey),
            nameof(IReportCoverage.CodecovToken),
        },
        OnPullRequestExcludePaths = new[]
        {
            "docs/*",
            "README.md",
            "CHANGELOG.md",
            "LICENSE"
        }
    )]
    [GitHubActions(
        "delivery",
        GitHubActionsImage.UbuntuLatest,
        FetchDepth = 0,
        OnPushBranches = new[] { IHaveMainBranch.MainBranchName, IGitFlow.ReleaseBranch + "/*" },
        InvokedTargets = new[] { nameof(IUnitTest.UnitTests), nameof(IPublish.Publish), nameof(ICreateGithubRelease.AddGithubRelease) },
        EnableGitHubToken = true,
        CacheKeyFiles = new[] { "global.json", "src/**/*.csproj" },
        PublishArtifacts = true,
        ImportSecrets = new[]
        {
            nameof(NugetApiKey),
            nameof(IReportCoverage.CodecovToken)
        },
        OnPullRequestExcludePaths = new[]
        {
            "docs/*",
            "README.md",
            "CHANGELOG.md",
            "LICENSE"
        }
    )]

    public class Build : NukeBuild,
        IHaveGitVersion,
        IHaveArtifacts,
        IHaveChangeLog,
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
        IPublish,
        ICreateGithubRelease
    {
        [Parameter("API key used to publish artifacts to Nuget.org")]
        [Secret]
        public readonly string NugetApiKey;

        [Solution]
        [Required]
        public readonly Solution Solution;

        Solution IHaveSolution.Solution => Solution;

        ///<inheritdoc/>
        public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);

        ///<inheritdoc/>
        IEnumerable<AbsolutePath> IClean.DirectoriesToDelete => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobDirectories("**/bin", "**/obj")
            .Concat(this.Get<IHaveTestDirectory>().TestDirectory.GlobDirectories("**/bin", "**/obj"));

        ///<inheritdoc/>
        AbsolutePath IHaveSourceDirectory.SourceDirectory => RootDirectory / "src";

        ///<inheritdoc/>
        AbsolutePath IHaveTestDirectory.TestDirectory => RootDirectory / "test";

        ///<inheritdoc/>
        IEnumerable<Project> IUnitTest.UnitTestsProjects => this.Get<IHaveSolution>().Solution.GetProjects("*UnitTests");

        ///<inheritdoc/>
        IEnumerable<Project> IMutationTest.MutationTestsProjects => this.Get<IUnitTest>().UnitTestsProjects;

        ///<inheritdoc/>
        IEnumerable<Project> IBenchmark.BenchmarkProjects => this.Get<IHaveSolution>().Solution.GetProjects("*.PerfomanceTests");

        ///<inheritdoc/>
        bool IReportCoverage.ReportToCodeCov => this.Get<IReportCoverage>().CodecovToken is not null;

        ///<inheritdoc/>
        IEnumerable<AbsolutePath> IPack.PackableProjects => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");

        ///<inheritdoc/>
        IEnumerable<PublishConfiguration> IPublish.PublishConfigurations => new PublishConfiguration[]
        {
            new NugetPublishConfiguration(apiKey: NugetApiKey,
                                          source: new Uri("https://api.nuget.org/v3/index.json"),
                                          () => NugetApiKey is not null),
            new GitHubPublishConfiguration(githubToken: this.Get<IHaveGitHubRepository>().GitHubToken,
                                           source: new Uri("https://nukpg.github.com/"),
                                           () => this is ICreateGithubRelease && this.Get<ICreateGithubRelease>()?.GitHubToken is not null)
        };
    }
}