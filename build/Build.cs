using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild {
    public static int Main() => Execute<Build>(x => x.Compile);

    [GitRepository] readonly GitRepository Repository = null!;
    [Solution(GenerateProjects = true)] readonly Solution Solution = null!;
    [PathVariable] readonly Tool Git = null!;

    [Parameter($"NuGet API key - Required for target {nameof(Publish)}"), Secret]
    readonly string NugetApiKey = null!;

    string SemVer => Repository.Tags.SingleOrError($"Last commit {Repository.Commit[..7]} wasn't tagged.");
    string PrevSemVer => Git("describe --tags --abbrev=0 HEAD^").Single().Text;
    string CommitMsgsSinceLastSemVer => Git($"log {PrevSemVer}..HEAD --format=%s").Select(o => o.Text).Join(Environment.NewLine);

    AbsolutePath ArtifactsDir => RootDirectory / "artifacts";
    AbsolutePath PackagePath => ArtifactsDir / $"{Solution.Json5.Name}.{SemVer}.nupkg";

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
            .SetNoBuild(true)));

    Target Pack => t => t
        .DependsOn(Clean, Compile)
        .Executes(() => DotNetPack(opts => opts
            .SetProject(Solution.Json5)
            .SetNoBuild(true)
            .SetVersion(SemVer)
            .SetPackageId("JSON5")
            .SetTitle("JSON5")
            .SetDescription("JSON5 for your dotnet appsettings files.")
            .SetPackageTags("JSON5 JSON parser translator deserializer appsettings configuration hosting")
            .SetPackageReleaseNotes(CommitMsgsSinceLastSemVer)
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
        .Requires(() => Repository.IsOnMainBranch())
        .Requires(() => NugetApiKey)
        .Executes(() => /*DotNetNuGetPush(opts => opts
            .SetTargetPath(PackagePath)
            .SetSource("https://www.nuget.org/")
            .SetApiKey(NugetApiKey)));*/
            Log.Information($"{string.Join(",", Repository.Tags)} dotnet nuget push {PackagePath} --source https://www.nuget.org/ --api-key {NugetApiKey}"));
}
