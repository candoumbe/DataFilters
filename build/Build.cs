namespace DataFilters.ContinuousIntegration
{
    using Nuke.Common;
    using Nuke.Common.CI;
    using Nuke.Common.CI.AzurePipelines;
    using Nuke.Common.CI.GitHubActions;
    using Nuke.Common.Execution;
    using Nuke.Common.Git;
    using Nuke.Common.IO;
    using Nuke.Common.ProjectModel;
    using Nuke.Common.Tooling;
    using Nuke.Common.Tools.Coverlet;
    using Nuke.Common.Tools.DotNet;
    using Nuke.Common.Tools.GitVersion;
    using Nuke.Common.Tools.Codecov;
    using Nuke.Common.Tools.ReportGenerator;
    using Nuke.Common.Utilities;
    using Nuke.Common.Tools.GitReleaseManager;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using static Nuke.Common.ChangeLog.ChangelogTasks;
    using static Nuke.Common.IO.FileSystemTasks;
    using static Nuke.Common.IO.PathConstruction;
    using static Nuke.Common.Logger;
    using static Nuke.Common.Tools.DotNet.DotNetTasks;
    using static Nuke.Common.Tools.Git.GitTasks;
    using static Nuke.Common.Tools.GitVersion.GitVersionTasks;
    using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
    using static Nuke.Common.Tools.Codecov.CodecovTasks;

    [GitHubActions(
        "integration",
        GitHubActionsImage.WindowsLatest, GitHubActionsImage.MacOsLatest,
        OnPushBranchesIgnore = new[] { MainBranchName },
        OnPullRequestBranches = new[] { DevelopBranch },
        PublishArtifacts = true,
        InvokedTargets = new[] { nameof(Tests), nameof(Pack) },
        ImportSecrets = new[]
        {
            nameof(NugetApiKey),
            nameof(CodecovToken)
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
        GitHubActionsImage.WindowsLatest, GitHubActionsImage.MacOsLatest,
        OnPushBranches = new[] { MainBranchName, ReleaseBranchPrefix + "/*" },
        InvokedTargets = new[] { nameof(Publish), nameof(AddGithubRelease) },
        ImportGitHubTokenAs = nameof(GitHubToken),
        PublishArtifacts = true,
        ImportSecrets = new[]
        {
            nameof(NugetApiKey),
            nameof(CodecovToken)
        },
        OnPullRequestExcludePaths = new[]
        {
            "docs/*",
            "README.md",
            "CHANGELOG.md",
            "LICENSE"
        }
    )]
    [CheckBuildProjectConfigurations]
    [UnsetVisualStudioEnvironmentVariables]
    [DotNetVerbosityMapping]
    [HandleVisualStudioDebugging]
    public class Build : NukeBuild
    {
        public static int Main() => Execute<Build>(x => x.Compile);

        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Parameter("Indicates wheter to restore nuget in interactive mode - Default is false")]
        public readonly bool Interactive = false;

        [Parameter("Generic name placeholder. Can be used wherever a name is required")]
        public readonly string Name;

        [Parameter]
        [Secret]
        public readonly string GitHubToken;

        [Parameter]
        [Secret]
        public readonly string CodecovToken;

        [Required] [Solution] public readonly Solution Solution;
        [Required] [GitRepository] public readonly GitRepository GitRepository;
        [Required] [GitVersion(Framework = "net5.0")] public readonly GitVersion GitVersion;

        [CI] public readonly AzurePipelines AzurePipelines;
        [CI] public readonly GitHubActions GitHubActions;

        [Partition(3)] public readonly Partition TestPartition;

        /// <summary>
        /// Directory of source code projects
        /// </summary>
        public AbsolutePath SourceDirectory => RootDirectory / "src";

        /// <summary>
        /// Directory of test projects
        /// </summary>
        public AbsolutePath TestDirectory => RootDirectory / "test";

        /// <summary>
        /// Directory where to store all output builds output
        /// </summary>
        public AbsolutePath OutputDirectory => RootDirectory / "output";

        /// <summary>
        /// Directory where to pu
        /// </summary>
        public AbsolutePath CoverageReportDirectory => OutputDirectory / "coverage-report";

        /// <summary>
        /// Directory where to publish all test results
        /// </summary>
        public AbsolutePath TestResultDirectory => OutputDirectory / "tests-results";

        /// <summary>
        /// Directory where to publish all artifacts
        /// </summary>
        public AbsolutePath ArtifactsDirectory => OutputDirectory / "artifacts";

        /// <summary>
        /// Directory where to publish converage history report
        /// </summary>
        public AbsolutePath CoverageReportHistoryDirectory => OutputDirectory / "coverage-history";

        /// <summary>
        /// Directory where to publish benchmark results.
        /// </summary>
        public AbsolutePath BenchmarkDirectory => OutputDirectory / "benchmarks";

        public const string MainBranchName = "main";

        public const string DevelopBranch = "develop";

        public const string FeatureBranchPrefix = "feature";

        public const string HotfixBranchPrefix = "hotfix";

        public const string ColdfixBranchPrefix = "coldfix";

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

                DotNetToolRestore();
            });

        public Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetNoRestore(InvokedTargets.Contains(Restore) || SkippedTargets.Contains(Restore))
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
            .Triggers(ReportCoverage)
            .Executes(() =>
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
                    .SetCoverletOutputFormat(CoverletOutputFormat.lcov)
                    .AddProperty("ExcludeByAttribute", "Obsolete")
                    .CombineWith(testsProjects, (cs, project) => cs.SetProjectFile(project)
                                                                   .CombineWith(project.GetTargetFrameworks(), (setting, framework) => setting.SetFramework(framework)
                                                                                                                                              .AddLoggers($"trx;LogFileName={project.Name}.trx")
                                                                                                                                              .SetCoverletOutput(TestResultDirectory / $"{project.Name}.{framework}.xml")))
                    );

                TestResultDirectory.GlobFiles("*.trx")
                                        .ForEach(testFileResult => AzurePipelines?.PublishTestResults(type: AzurePipelinesTestResultsType.VSTest,
                                                                                                        title: $"{Path.GetFileNameWithoutExtension(testFileResult)} ({AzurePipelines.StageDisplayName})",
                                                                                                        files: new string[] { testFileResult })
                        );

                TestResultDirectory.GlobFiles("*.xml")
                                .ForEach(file => AzurePipelines?.PublishCodeCoverage(coverageTool: AzurePipelinesCodeCoverageToolType.Cobertura,
                                                                                        summaryFile: file,
                                                                                        reportDirectory: CoverageReportDirectory));
            });

        public Target ReportCoverage => _ => _
            .DependsOn(Tests)
            .Requires(() => IsServerBuild || CodecovToken != null)
            .Consumes(Tests, TestResultDirectory / "*.xml")
            .Produces(CoverageReportDirectory / "*.xml")
            .Produces(CoverageReportHistoryDirectory / "*.xml")
            .Executes(() =>
            {
                ReportGenerator(_ => _
                        .SetFramework("net5.0")
                        .SetReports(TestResultDirectory / "*.xml")
                        .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlChart, ReportTypes.HtmlInline_AzurePipelines_Dark)
                        .SetTargetDirectory(CoverageReportDirectory)
                        .SetHistoryDirectory(CoverageReportHistoryDirectory)
                        .SetTag(GitRepository.Commit)
                    );

                Codecov(s => s
                    .SetFiles(CoverageReportDirectory / "*.xml")
                    .SetToken(CodecovToken)
                    .SetBranch(GitRepository.Branch)
                    .SetSha(GitRepository.Commit)
                    .SetBuild(GitVersion.FullSemVer)
                    .SetFramework("net5.0")
                );
            });

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
                    .SetRepositoryType("git")
                    .SetRepositoryUrl(GitRepository.HttpsUrl)
                );
            });

        private AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";

        #region Git flow section

        public Target Changelog => _ => _
            .Requires(() => IsLocalBuild)
            .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch() || GitRepository.IsOnHotfixBranch())
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
                    AskBranchNameAndSwitchToIt(FeatureBranchPrefix, DevelopBranch);
#pragma warning restore S2583 // Conditionally executed code should be reachable

                    Info($"{EnvironmentInfo.NewLine}Good bye !");
                }
                else
                {
                    FinishFeature();
                }
            });

        /// <summary>
        /// Asks the user for a branch name
        /// </summary>
        /// <param name="branchNamePrefix">A prefix to preprend in front of the user branch name</param>
        /// <param name="sourceBranch">Branch from which a new branch will be created</param>
        private void AskBranchNameAndSwitchToIt(string branchNamePrefix, string sourceBranch)
        {
            string featureName;
            bool exitCreatingFeature = false;
            do
            {
                featureName = (Name ?? Console.ReadLine() ?? string.Empty).Trim()
                                                                .Trim('/');

                switch (featureName)
                {
                    case string name when !string.IsNullOrWhiteSpace(name):
                        {
                            string branchName = $"{branchNamePrefix}/{featureName.Slugify()}";
                            Info($"{Environment.NewLine}The branch '{branchName}' will be created.{Environment.NewLine}Confirm ? (Y/N) ");

                            switch (Console.ReadKey().Key)
                            {
                                case ConsoleKey.Y:
                                    Info($"{Environment.NewLine}Checking out branch '{branchName}' from '{sourceBranch}'");

                                    Checkout(branchName, start: sourceBranch);

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
                        Info($"Exiting task.");
                        exitCreatingFeature = true;
                        break;
                }

#pragma warning disable S2583 // Conditionally executed code should be reachable
            } while (string.IsNullOrWhiteSpace(featureName) && !exitCreatingFeature);
        }

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

        public Target Coldfix => _ => _
            .Description($"Starts a new coldfix development by creating the associated '{ColdfixBranchPrefix}/{{name}}' from {DevelopBranch}")
            .Requires(() => IsLocalBuild)
            .Requires(() => !GitRepository.Branch.Like($"{ColdfixBranchPrefix}/*", true) || GitHasCleanWorkingCopy())
            .Executes(() =>
            {
                if (!GitRepository.Branch.Like($"{ColdfixBranchPrefix}/*"))
                {
                    Info("Enter the name of the coldfix. It will be used as the name of the coldfix/branch (leave empty to exit) :");
                    AskBranchNameAndSwitchToIt(ColdfixBranchPrefix, DevelopBranch);
#pragma warning restore S2583 // Conditionally executed code should be reachable
                    Info($"{EnvironmentInfo.NewLine}Good bye !");
                }
                else
                {
                    FinishColdfix();
                }
            });

        /// <summary>
        /// Merge a coldfix/* branch back to the develop branch
        /// </summary>
        private void FinishColdfix() => FinishFeature();

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
            Git($"checkout {MainBranchName}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");
            Git($"tag {MajorMinorPatchVersion}");

            Git($"checkout {DevelopBranch}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");

            Git($"branch -D {GitRepository.Branch}");

            Git($"push origin --follow-tags {MainBranchName} {DevelopBranch} {MajorMinorPatchVersion}");
        }

        private void FinishFeature()
        {
            Git($"rebase {DevelopBranch}");
            Git($"checkout {DevelopBranch}");
            Git($"merge --no-ff --no-edit {GitRepository.Branch}");

            Git($"branch -D {GitRepository.Branch}");
            Git($"push origin {DevelopBranch}");
        }


        #endregion

        [Parameter("API key used to publish artifacts to Nuget.org")]
        [Secret]
        public readonly string NugetApiKey;

        [Parameter(@"URI where packages should be published (default : ""https://api.nuget.org/v3/index.json""")]
        public string NugetPackageSource => "https://api.nuget.org/v3/index.json";

        public string GitHubPackageSource => $"https://nuget.pkg.github.com/{GitHubActions.GitHubRepositoryOwner}/index.json";

        public bool IsOnGithub => GitHubActions is not null;

        public Target Publish => _ => _
            .Description($"Published packages (*.nupkg and *.snupkg) to the destination server set with {nameof(NugetPackageSource)} settings ")
            .DependsOn(Tests, Pack)
            .Triggers(AddGithubRelease)
            .Consumes(Pack, ArtifactsDirectory / "*.nupkg", ArtifactsDirectory / "*.snupkg")
            .Requires(() => !(NugetApiKey.IsNullOrEmpty() || GitHubToken.IsNullOrEmpty()))
            .Requires(() => GitHasCleanWorkingCopy())
            .Requires(() => GitRepository.IsOnMainBranch()
                            || GitRepository.IsOnReleaseBranch()
                            || GitRepository.IsOnDevelopBranch())
            .Requires(() => Configuration.Equals(Configuration.Release))
            .Executes(() =>
            {
                void PushPackages(IReadOnlyCollection<AbsolutePath> nupkgs)
                {
                    Info($"Publishing {nupkgs.Count} package{(nupkgs.Count > 1 ? "s" : string.Empty)}");
                    Info(string.Join(EnvironmentInfo.NewLine, nupkgs));

                    DotNetNuGetPush(s => s.SetApiKey(NugetApiKey)
                        .SetSource(NugetPackageSource)
                        .EnableSkipDuplicate()
                        .EnableNoSymbols()
                        .SetProcessLogTimestamp(true)
                        .CombineWith(nupkgs, (_, nupkg) => _
                                    .SetTargetPath(nupkg)),
                        degreeOfParallelism: 4,
                        completeOnFailure: true);

                    DotNetNuGetPush(s => s.SetApiKey(GitHubToken)
                        .SetSource(GitHubPackageSource)
                        .EnableSkipDuplicate()
                        .EnableNoSymbols()
                        .SetProcessLogTimestamp(true)
                        .CombineWith(nupkgs, (_, nupkg) => _
                                    .SetTargetPath(nupkg)),
                        degreeOfParallelism: 4,
                        completeOnFailure: true);
                }

                PushPackages(ArtifactsDirectory.GlobFiles("*.nupkg", "!*TestObjects.*nupkg", "!*PerformanceTests.*nupkg"));
                PushPackages(ArtifactsDirectory.GlobFiles("*.snupkg", "!*TestObjects.*nupkg", "!*PerformanceTests.*nupkg"));
            });

        public Target AddGithubRelease => _ => _
            .After(Publish)
            .Unlisted()
            .Description("Creates a new GitHub release after *.nupkgs/*.snupkg were successfully published.")
            .OnlyWhenStatic(() => IsServerBuild && GitRepository.IsOnMainBranch())
            .Executes(async () =>
            {
                Info("Creating a new release");
                Octokit.GitHubClient gitHubClient = new(new Octokit.ProductHeaderValue(nameof(DataFilters)))
                {
                    Credentials = new Octokit.Credentials(GitHubToken)
                };

                string repositoryName = GitHubActions.GitHubRepository.Replace(GitHubActions.GitHubRepositoryOwner + "/", string.Empty);
                IReadOnlyList<Octokit.Release> releases = await gitHubClient.Repository.Release.GetAll(GitHubActions.GitHubRepositoryOwner, repositoryName)
                                                                                               .ConfigureAwait(false);

                if (!releases.AtLeastOnce(release => release.Name == MajorMinorPatchVersion))
                {
                    Octokit.NewRelease newRelease = new(MajorMinorPatchVersion)
                    {
                        TargetCommitish = GitRepository.Commit,
                        Body = string.Concat("- ", ExtractChangelogSectionNotes(ChangeLogFile, MajorMinorPatchVersion).Select(line => $"{line}\n")),
                        Name = MajorMinorPatchVersion,
                    };

                    Octokit.Release release = await gitHubClient.Repository.Release.Create(GitHubActions.GitHubRepositoryOwner, repositoryName, newRelease)
                                                                                   .ConfigureAwait(false);

                    Info($"Github release {release.TagName} created successfully");
                }
                else
                {
                    Info($"Release '{MajorMinorPatchVersion}' already exists - skipping ");
                }
            });

        public Target Benchmarks => _ => _
            .Description("Run all performance tests.")
            .DependsOn(Compile)
            .Produces(BenchmarkDirectory / "*")
            .OnlyWhenStatic(() => IsLocalBuild)
            .Executes(() =>
            {
                IEnumerable<Project> benchmarkProjects = Solution.GetProjects("*.PerformanceTests");

                benchmarkProjects.ForEach(csproj =>
                {
                    DotNetRun(s =>
                    {
                        IReadOnlyCollection<string> frameworks = csproj.GetTargetFrameworks();
                        return s.SetConfiguration(Configuration.Release)
                                                            .SetProjectFile(csproj)
                                                            .SetProcessArgumentConfigurator(args => args.Add("-- --filter {0}", "*", customValue: true)
                                                                                                        .Add("--artifacts {0}", BenchmarkDirectory)
                                                                                                        .Add("--join"))
                                                            .CombineWith(frameworks, (setting, framework) => setting.SetFramework(framework));
                    });
                });
            });
    }
}