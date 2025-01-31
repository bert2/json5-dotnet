using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild {
    public static int Main() => Execute<Build>(x => x.Compile);

    [GitRepository] readonly GitRepository Repository = null!;
    [Solution(GenerateProjects = true)] readonly Solution Solution = null!;

    [Parameter($"NuGet API key - Required for target {nameof(Publish)}"), Secret]
    readonly string NugetApiKey = null!;

    string SemVer => Repository.Tags.SingleOrDefault(IsSemVer) ?? "0.0.0";

    AbsolutePath ArtifactsDir => RootDirectory / "artifacts";
    AbsolutePath PackagePath => ArtifactsDir / $"{Solution.Json5.Name}.{SemVer}.nupkg";

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
        .DependsOn(Compile)
        .Executes(() => DotNetPack(opts => opts
            .SetProject(Solution.Json5)
            .SetNoBuild(true)
            .SetVersion(SemVer)
            .SetOutputDirectory(ArtifactsDir)));

    Target Publish => t => t
        .DependsOn(Test, Pack)
        .Requires(() => Repository.IsOnMainBranch())
        .Requires(() => LastCommitHasSemVerTag())
        .Requires(() => NugetApiKey)
        .Executes(() => /*DotNetNuGetPush(opts => opts
            .SetTargetPath(PackagePath)
            .SetSource("https://www.nuget.org/")
            .SetApiKey(NugetApiKey)));*/
            Log.Information($"{string.Join(",", Repository.Tags)} dotnet nuget push {PackagePath} --source https://www.nuget.org/ --api-key {NugetApiKey}"));

    bool IsSemVer(string s)
        => Version.TryParse(s, out var v)
        && v is { Major: not -1, Minor: not -1, Build: not -1, Revision: -1, MajorRevision: -1, MinorRevision: -1 };

    bool LastCommitHasSemVerTag() {
        var sha = Repository.Commit;
        var tags = Repository.Tags.Join(", ");

        switch (Repository.Tags.Count(IsSemVer)) {
            case 0:
                Log.Error($"No semver tag found on last commit ({sha}). Existing tags: [{tags}]");
                return false;
            case 1:
                return true;
            default:
                Log.Error($"Multiple semver tags found on last commit ({sha}). Existing tags: [{tags}]");
                return false;
        }
    }
}
