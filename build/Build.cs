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

    [GitRepository] readonly GitRepository Repository = null!;
    [PathVariable] readonly Tool Git = null!;
    [Solution(GenerateProjects = true)] readonly Solution Solution = null!;

    [Parameter($"NuGet API key - Required for target {nameof(Publish)}"), Secret]
    readonly string NugetApiKey = null!;

    string? lastGitTag;
    string LastGitTag => lastGitTag ??= Git("describe --tags --abbrev=0", logOutput: false).Single().Text;
    bool LastCommitHasVersionTag => Repository.Tags.Contains(LastGitTag) && Version.TryParse(LastGitTag, out var _);

    AbsolutePath ArtifactsDir => RootDirectory / "artifacts";
    AbsolutePath PackagePath => ArtifactsDir / $"{Solution.Json5.Name}.{LastGitTag}.nupkg";

    Target Compile => t => t
        .Executes(() => DotNetBuild(opts => opts
            .SetConfiguration("Release")
            .SetVersion(LastGitTag)));

    Target Test => t => t
        .DependsOn(Compile)
        .Executes(() => DotNetTest(opts => opts
            .SetProjectFile(Solution.Json5_Tests)
            .SetNoBuild(true)));

    Target Pack => t => t
        .DependsOn(Compile)
        .Executes(() => DotNetPack(opts => opts
            .SetProject(Solution.Json5)
            .SetNoBuild(true)
            .SetVersion(LastGitTag)
            .SetOutputDirectory(ArtifactsDir)));

    Target Publish => t => t
        .DependsOn(Test, Pack)
        .Requires(() => Repository.IsOnMainBranch())
        .Requires(() => LastCommitHasVersionTag)
        .Requires(() => NugetApiKey)
        .Executes(() => /*DotNetNuGetPush(opts => opts
            .SetTargetPath(PackagePath)
            .SetSource("https://www.nuget.org/")
            .SetApiKey(NugetApiKey)));*/
            Log.Information($"{string.Join(",", Repository.Tags)} dotnet nuget push {PackagePath} --source https://www.nuget.org/ --api-key {NugetApiKey}"));
}
