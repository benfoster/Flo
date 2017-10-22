//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool "nuget:?package=GitVersion.CommandLine"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var isCIBuild = !BuildSystem.IsLocalBuild;

var projects = "./src/**/*.csproj";
var testProjects = "./test/**/*.csproj";

var allProjectFiles = GetFiles(projects) + GetFiles(testProjects);

var packFiles = "./src/Flo/*.csproj";
var buildArtifacts = "./artifacts";

GitVersion gitVersionInfo;
string nugetVersion;

Setup(context =>
{
    gitVersionInfo = GitVersion(new GitVersionSettings {
        OutputType = GitVersionOutput.Json
    });

    nugetVersion = gitVersionInfo.NuGetVersion;
    
    Information("Building Flo v{0} with configuration {1}", nugetVersion, configuration);
});

Task("__Clean")
    .Does(() => 
    {
        CleanDirectories(buildArtifacts);
    });

Task("__Restore")
    .Does(() =>
    {      
        foreach (var projectFile in allProjectFiles)
        {
            DotNetCoreRestore(projectFile.ToString());
        }
    });

Task("__Build")
    .Does(() =>
    {
        var settings = new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            ArgumentCustomization = args => args.Append($"/p:Version={nugetVersion}")
        };
        
        foreach (var projectFile in allProjectFiles)
        {
            DotNetCoreBuild(projectFile.ToString(), settings);
        }
    });

Task("__Test")
    .Does(() =>
    {
        foreach (var projectFile in GetFiles(testProjects))
        {
            DotNetCoreRun(projectFile.ToString());
        }
    });

Task("__Pack")
    .Does(() => 
    {
        var settings = new DotNetCorePackSettings 
        {
            Configuration = configuration,
            OutputDirectory = buildArtifacts,
            NoBuild = true,
            ArgumentCustomization = args => args.Append($"/p:Version={nugetVersion}")
        };
        
        foreach (var projectFile in GetFiles(packFiles))
        {
            DotNetCorePack(projectFile.ToString(), settings); 
        }
    });

Task("__PublishNuget")
    .WithCriteria(() => ShouldPublish(Context))
    .Does(() => 
    {
        // // Resolve the API key.
        var apiKey = EnvironmentVariable("NUGET_API_KEY");
        if(string.IsNullOrEmpty(apiKey)) {
            throw new InvalidOperationException("Could not resolve NuGet API key.");
        }

        // Resolve the API url.
        var apiUrl = EnvironmentVariable("NUGET_API_URL");
        if(string.IsNullOrEmpty(apiUrl)) {
            throw new InvalidOperationException("Could not resolve NuGet API url.");
        }

        foreach(var package in GetFiles("./artifacts/*.nupkg"))
        {
            // Push the package.
            NuGetPush(package.ToString(), new NuGetPushSettings {
                ApiKey = apiKey,
                Source = apiUrl
            });
        }
    });

private static bool ShouldPublish(ICakeContext context)
{
    var buildSystem = context.BuildSystem();

    return buildSystem.AppVeyor.IsRunningOnAppVeyor
        && buildSystem.AppVeyor.Environment.Repository.Tag.IsTag
        && !string.IsNullOrWhiteSpace(buildSystem.AppVeyor.Environment.Repository.Tag.Name);
}

Task("Build")
    .IsDependentOn("__Clean")
    .IsDependentOn("__Restore")
    .IsDependentOn("__Build")
    .IsDependentOn("__Test")
    .IsDependentOn("__Pack");

// Task("Deploy")
//     .IsDependentOn("Build");

Task("Default")
    .IsDependentOn("Build");

Task("AppVeyor")
    .IsDependentOn("Build")
    .IsDependentOn("__PublishNuget");

RunTarget(target);