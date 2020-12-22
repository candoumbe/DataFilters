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
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.GitVersion.GitVersionTasks;

[AzurePipelines(
    suffix: "release",
    AzurePipelinesImage.WindowsLatest,
    InvokedTargets = new[] { nameof(Pack) },
    NonEntryTargets = new[] { nameof(Restore), nameof(Changelog) },
    ExcludedTargets = new[] { nameof(Clean) },
    PullRequestsAutoCancel = true,
    TriggerBranchesInclude = new[] { ReleaseBranchPrefix + "/*" },
    TriggerPathsExclude = new[]
    {
        "docs/*",
        "README.md",
        "CHANGELOG.md"
    }
)]
[AzurePipelines(
    suffix: "pull-request",
    AzurePipelinesImage.WindowsLatest,
    InvokedTargets = new[] { nameof(Tests) },
    NonEntryTargets = new[] { nameof(Restore), nameof(Changelog) },
    ExcludedTargets = new[] { nameof(Clean) },
    PullRequestsAutoCancel = true,
    PullRequestsBranchesInclude = new[] { MainBranchName },
    TriggerBranchesInclude = new[] {
        FeatureBranchPrefix + "/*",
        HotfixBranchPrefix + "/*"
    },
    TriggerPathsExclude = new[]
    {
        "docs/*",
        "README.md",
        "CHANGELOG.md"
    }
)]
[AzurePipelines(
    AzurePipelinesImage.WindowsLatest,
    InvokedTargets = new[] { nameof(Pack) },
    NonEntryTargets = new[] { nameof(Restore), nameof(Changelog) },
    ExcludedTargets = new[] { nameof(Clean) },
    PullRequestsAutoCancel = true,
    TriggerBranchesInclude = new[] { MainBranchName },
    TriggerPathsExclude = new[]
    {
        "docs/*",
        "README.md",
        "CHANGELOG.md"
    }
)]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
public class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    [Parameter("Indicates wheter to restore nuget in interactive mode - Default is false")]
    public readonly bool Interactive = false;

    [Required] [Solution] public readonly Solution Solution;
    [Required] [GitRepository] public readonly GitRepository GitRepository;
    [Required] [GitVersion(Framework = "net5.0", NoFetch = true)] public readonly GitVersion GitVersion;
    [CI] public readonly AzurePipelines AzurePipelines;

    [Partition(3)] public readonly Partition TestPartition;

    public AbsolutePath SourceDirectory => RootDirectory / "src";

    public AbsolutePath TestDirectory => RootDirectory / "test";

    public AbsolutePath OutputDirectory => RootDirectory / "output";

    public AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";

    public AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

    public AbsolutePath ArtifactsDirectory => OutputDirectory / "artifacts";

    public AbsolutePath CoverageReportHistoryDirectory => OutputDirectory / "coverage-history";

    public const string MainBranchName = "main";

    public const string DevelopBranch = "develop";

    public const string FeatureBranchPrefix = "feature";

    public const string HotfixBranchPrefix = "hotfix";

    public const string ReleaseBranchPrefix = "release";

    [Parameter("Indicates if any changes should be stashed automatically prior to switching branch (Default : true)")]
    public readonly bool AutoStash = true;

    public Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
            EnsureCleanDirectory(CoverageReportDirectory);
            EnsureExistingDirectory(CoverageReportHistoryDirectory);
        });

    public Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetIgnoreFailedSources(true)
                .SetDisableParallel(false)
                .When(IsLocalBuild && Interactive, _ => _.SetProperty("NugetInteractive", IsLocalBuild && Interactive))
            );
        });

    public Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetNoRestore(InvokedTargets.Contains(Restore))
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                );
        });

    public Target Tests => _ => _
        .DependsOn(Compile)
        .Description("Run unit tests and collect code coverage")
        .Produces(TestResultDirectory / "*.trx")
        .Produces(TestResultDirectory / "*.xml")
        .Produces(CoverageReportHistoryDirectory / "*.xml")
        .Executes(() =>
        {
            RunTests();
        });

    private void RunTests()
    {
        IEnumerable<Project> projects = Solution.GetProjects("*.UnitTests");
        IEnumerable<Project> testsProjects = TestPartition.GetCurrent(projects);

        testsProjects.ForEach(project => Info(project));

        DotNetTest(s => s
            .SetConfiguration(Configuration)
            .EnableCollectCoverage()
            .EnableUseSourceLink()
            .SetNoBuild(InvokedTargets.Contains(Compile))
            .SetResultsDirectory(TestResultDirectory)
            .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
            .AddProperty("ExcludeByAttribute", "Obsolete")
            .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting
                    .SetFramework(framework)
                    .SetLogger($"trx;LogFileName={project.Name}.{framework}.trx")
                    .SetCollectCoverage(true)
                    .SetCoverletOutput(TestResultDirectory / $"{project.Name}.xml"))
                )
        );

        TestResultDirectory.GlobFiles("*.trx")
                           .ForEach(testFileResult => AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                                                         title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                                                         files: new string[] { testFileResult })
        );

        // TODO Move this to a separate "coverage" target once https://github.com/nuke-build/nuke/issues/562 is solved !
        ReportGenerator(_ => _
            .SetFramework("net5.0")
            .SetReports(TestResultDirectory / "*.xml")
            .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlChart, ReportTypes.HtmlInline_AzurePipelines_Dark)
            .SetTargetDirectory(CoverageReportDirectory)
            .SetHistoryDirectory(CoverageReportHistoryDirectory)
        );

        TestResultDirectory.GlobFiles("*.xml")
                           .ForEach(file => AzurePipelines?.PublishCodeCoverage(coverageTool: AzurePipelinesCodeCoverageToolType.Cobertura,
                                                                                summaryFile: file,
                                                                                reportDirectory: CoverageReportDirectory));
    }

    public Target Pack => _ => _
        .DependsOn(Tests, Compile)
        .Consumes(Compile)
        .Produces(ArtifactsDirectory / "*.nupkg")
        .Produces(ArtifactsDirectory / "*.snupkg")
        .Executes(() =>
        {
            DotNetPack(s => s
                .EnableIncludeSource()
                .EnableIncludeSymbols()
                .SetOutputDirectory(ArtifactsDirectory)
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetVersion(GitVersion.NuGetVersion)
                .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                .SetPackageReleaseNotes(GetNuGetReleaseNotes(ChangeLogFile, GitRepository))
            );
        });

    private AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";

    #region Git flow section

    public Target Changelog => _ => _
        .Requires(() => IsLocalBuild)
        .Requires(() => !GitRepository.IsOnReleaseBranch() || GitHasCleanWorkingCopy())
        .Description("Finalizes the change log so that its up to date for the release. ")
        .Executes(() =>
        {
            FinalizeChangelog(ChangeLogFile, GitVersion.MajorMinorPatch, GitRepository);
            Info($"Please review CHANGELOG.md ({ChangeLogFile}) and press 'Y' to validate (any other key will cancel changes)...");
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (keyInfo.Key == ConsoleKey.Y)
            {
                Git($"add {ChangeLogFile}");
                Git($"commit -m \"Finalize {Path.GetFileName(ChangeLogFile)} for {GitVersion.MajorMinorPatch}\"");
            }
        });

    public Target Feature => _ => _
        .Description($"Starts a new feature development by creating the associated branch {FeatureBranchPrefix}/{{feature-name}} from {DevelopBranch}")
        .Requires(() => IsLocalBuild)
        .Requires(() => !GitRepository.IsOnFeatureBranch() || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            if (!GitRepository.IsOnFeatureBranch())
            {
                Info("Enter the name of the feature. It will be used as the name of the feature/branch (leave empty to exit) :");
                string featureName;
                bool exitCreatingFeature = false;
                do
                {
                    featureName = (Console.ReadLine() ?? string.Empty).Trim()
                                                                    .Trim('/');

                    switch (featureName)
                    {
                        case string name when !string.IsNullOrWhiteSpace(name):
                            {
                                string branchName = $"{FeatureBranchPrefix}/{featureName.Slugify()}";
                                Info($"{Environment.NewLine}The branch '{branchName}' will be created.{Environment.NewLine}Confirm ? (Y/N) ");

                                switch (Console.ReadKey().Key)
                                {
                                    case ConsoleKey.Y:
                                        Info($"{Environment.NewLine}Checking out branch '{branchName}' from '{DevelopBranch}'");
                                        Checkout(branchName, start: DevelopBranch);
                                        Info($"{Environment.NewLine}'{branchName}' created successfully");
                                        exitCreatingFeature = true;
                                        break;

                                    default:
                                        Info($"{Environment.NewLine}Exiting {nameof(Feature)} task.");
                                        exitCreatingFeature = true;
                                        break;
                                }
                            }
                            break;
                        default:
                            Info($"Exiting {nameof(Feature)} task.");
                            exitCreatingFeature = true;
                            break;
                    }

                } while (string.IsNullOrWhiteSpace(featureName) && !exitCreatingFeature);

                Info($"{EnvironmentInfo.NewLine}Good bye !");
            }
            else
            {
                RunTests();
                FinishFeature();
            }
        });

    public Target Release => _ => _
        .DependsOn(Changelog)
        .Description($"Starts a new {ReleaseBranchPrefix}/{{version}} from {DevelopBranch}")
        .Requires(() => !GitRepository.IsOnReleaseBranch() || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            if (!GitRepository.IsOnReleaseBranch())
            {
                Checkout($"{ReleaseBranchPrefix}/{GitVersion.MajorMinorPatch}", start: DevelopBranch);
            }
            else
            {
                FinishReleaseOrHotfix();
            }
        });

    public Target Hotfix => _ => _
        .DependsOn(Changelog)
        .Description($"Starts a new hotfix branch '{HotfixBranchPrefix}/*' from {MainBranchName}")
        .Requires(() => !GitRepository.IsOnHotfixBranch() || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            (GitVersion mainBranchVersion, IReadOnlyCollection<Output> _) = GitVersion(s => s
                .SetFramework("net5.0")
                .SetUrl(RootDirectory)
                .SetBranch(MainBranchName)
                .EnableNoFetch()
                .DisableProcessLogOutput());

            if (!GitRepository.IsOnHotfixBranch())
            {
                Checkout($"{HotfixBranchPrefix}/{mainBranchVersion.Major}.{mainBranchVersion.Minor}.{mainBranchVersion.Patch + 1}", start: MainBranchName);
            }
            else
            {
                FinishReleaseOrHotfix();
            }
        });

    private void Checkout(string branch, string start)
    {
        bool hasCleanWorkingCopy = GitHasCleanWorkingCopy();

        if (!hasCleanWorkingCopy && AutoStash)
        {
            Git("stash");
        }
        Git($"checkout -b {branch} {start}");

        if (!hasCleanWorkingCopy && AutoStash)
        {
            Git("stash apply");
        }
    }

    private string MajorMinorPatchVersion => GitVersion.MajorMinorPatch;

    private void FinishReleaseOrHotfix()
    {
        RunTests();
        Git($"checkout {MainBranchName}");
        Git($"merge --no-ff --no-edit {GitRepository.Branch}");
        Git($"tag {MajorMinorPatchVersion}");

        Git($"checkout {DevelopBranch}");
        Git($"merge --no-ff --no-edit {GitRepository.Branch}");

        Git($"branch -D {GitRepository.Branch}");

        Git($"push origin {MainBranchName} {DevelopBranch} {MajorMinorPatchVersion}");
    }

    private void FinishFeature()
    {
        RunTests();
        Git($"checkout {DevelopBranch}");
        Git("rebase");

        Git($"branch -D {GitRepository.Branch}");
    }

    #endregion
}