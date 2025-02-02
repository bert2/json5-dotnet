using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "continuous",
    GitHubActionsImage.WindowsLatest,
    On = [GitHubActionsTrigger.Push],
    FetchDepth = 0,
    InvokedTargets = [nameof(Test), nameof(Pack)])]
[GitHubActions(
    "release",
    GitHubActionsImage.WindowsLatest,
    OnPushTags = ["[0-9]+.[0-9]+.[0-9]+"],
    FetchDepth = 0,
    ImportSecrets = [nameof(NuGetApiKey)],
    InvokedTargets = [nameof(Publish)])]
[GitHubActions(
    "pr",
    GitHubActionsImage.WindowsLatest,
    On = [GitHubActionsTrigger.PullRequest],
    FetchDepth = 0,
    InvokedTargets = [nameof(Test), nameof(Pack)])]
class Build : NukeBuild {
    public static int Main() => Execute<Build>(x => x.Compile);

    [GitRepository] readonly GitRepository Repository = null!;
    [Solution(GenerateProjects = true)] readonly Solution Solution = null!;
    [PathVariable] readonly Tool Git = null!;

    [Parameter($"NuGet API key - Required for target {nameof(Publish)}"), Secret]
    readonly string NuGetApiKey = null!;

    bool IsRelease => Version.TryParse(Repository.Tags.SingleOrDefault(), out _);

    string CurrSemVer => Repository.Tags.Single();

    string? prevSemVer;
    string PrevSemVer => prevSemVer ??= Git($"describe --tags --abbrev=0 HEAD^").Single().Text;

    string SemVer => IsRelease ? CurrSemVer : $"{PrevSemVer}-{Repository.Branch}-{Repository.Commit[..7]}";

    string? releaseNotes;
    string ReleaseNotes => releaseNotes ??= Git($"log {PrevSemVer}.. --format=%s").Select(o => o.Text).Join(Environment.NewLine);

    AbsolutePath ArtifactsDir => RootDirectory / "artifacts";
    AbsolutePath PackageFile => ArtifactsDir / $"JSON5.{SemVer}.nupkg";

    Target Clean => t => t
        .Executes(() => ArtifactsDir.DeleteDirectory());

    Target Compile => t => t
        .Executes(() => DotNetBuild(opts => opts
            .SetConfiguration("Release")
            .SetVersion(SemVer)));

    Target Test => t => t
        .DependsOn(Compile)
        .Executes(() => DotNetTest(opts => opts
            .SetProjectFile(Solution.Json5_Tests)
            .SetNoBuild(true)
            .SetConfiguration("Release")));

    Target Pack => t => t
        .DependsOn(Clean, Compile)
        .Produces(ArtifactsDir / "*.nupkg", ArtifactsDir / "*.snupkg")
        .Executes(() => DotNetPack(opts => opts
            .SetProject(Solution.Json5)
            .SetNoBuild(true)
            .SetVersion(SemVer)
            .SetPackageId("JSON5")
            .SetTitle("JSON5")
            .SetDescription("JSON5 for your dotnet appsettings files.")
            .SetPackageTags("JSON5 JSON parser translator deserializer appsettings configuration hosting")
            .SetPackageReleaseNotes(ReleaseNotes.Replace(";", "%3B").Replace(",", "%2C"))
            .SetAuthors("Robert Hofmann")
            .AddProcessAdditionalArguments("-p:PackageLicenseExpression=MIT")
            .SetRepositoryUrl("https://github.com/bert2/json5-dotnet.git")
            .SetIncludeSymbols(true)
            .SetIncludeSource(true)
            .AddProcessAdditionalArguments("-p:EmbedAllSources=true")
            .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
            .SetOutputDirectory(ArtifactsDir)));

    Target Publish => t => t
        .DependsOn(Test, Pack)
        .Requires(() => NuGetApiKey)
        .Requires(() => Repository.IsOnMainBranch() && IsRelease)
        .Executes(() => /*DotNetNuGetPush(opts => opts
            .SetTargetPath(PackageFile)
            .SetSource("https://api.nuget.org/v3/index.json")
            .SetApiKey(NugetApiKey)));*/
            Log.Information($"{string.Join(",", Repository.Tags)} dotnet nuget push {PackageFile} --source https://api.nuget.org/v3/index.json --api-key {NuGetApiKey}"));
}
