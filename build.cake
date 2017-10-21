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

RunTarget(target);