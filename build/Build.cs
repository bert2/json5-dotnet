using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild {
    public static int Main() => Execute<Build>(x => x.Compile);

    [GitRepository]
    readonly GitRepository Repository = null!;

    [PathVariable]
    readonly Tool Git = null!;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution = null!;

    [Parameter($"NuGet API key - Required for target {nameof(Publish)}"), Secret]
    readonly string NugetApiKey = null!;

    AbsolutePath ArtifactsDir => RootDirectory / "artifacts";

    AbsolutePath PackagePath => ArtifactsDir / $"{Solution.Json5.Name}.{LatestGitTag}.nupkg";

    string? latestGitTag;
    string LatestGitTag => latestGitTag ??= Git("describe --tags --abbrev=0", logOutput: false).Single().Text;

    Target Compile => _ => _
        .Executes(() => DotNetBuild(opts => opts
            .SetConfiguration("Release")
            .SetVersion(LatestGitTag)));

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() => DotNetTest(opts => opts
            .SetProjectFile(Solution.Json5_Tests)
            .SetNoBuild(true)));

    Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() => DotNetPack(opts => opts
            .SetProject(Solution.Json5)
            .SetNoBuild(true)
            .SetVersion(LatestGitTag)
            .SetOutputDirectory(ArtifactsDir)));

    Target Publish => _ => _
        .DependsOn(Test, Pack)
        .Requires(() => Repository.IsOnMainBranch())
        .Requires(() => Repository.Tags.Contains(LatestGitTag))
        .Requires(() => NugetApiKey)
        .Executes(() => /*DotNetNuGetPush(opts => opts
            .SetTargetPath(PackagePath)
            .SetSource("https://www.nuget.org/")
            .SetApiKey(NugetApiKey)));*/
            Log.Information($"dotnet nuget push {PackagePath} --source https://www.nuget.org/ --api-key {NugetApiKey}"));
}
