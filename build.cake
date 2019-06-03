// publish to NuGet
// dotnet pack --configuration Release
// dotnet nuget push .\bin\release\plr.1.0.2.nupkg --source https://api.nuget.org/v3/index.json --api-key {key}

// local run
// Start-Process -FilePath 'dotnet' -ArgumentList 'run --debug'

var target = Argument("target", "Default");
string api_key;
#l key.cake

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./artifacts/");
    DotNetCoreClean("./plr.sln");
});

Task("Pack")
  .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        Configuration = "Release",
        OutputDirectory = "./artifacts/"
    };
    DotNetCorePack("./plr.sln", settings);
});

Task("Push")
  .Does(() =>
{
    var settings = new DotNetCoreNuGetPushSettings
     {
         Source = "https://api.nuget.org/v3/index.json",
         ApiKey = api_key,
         WorkingDirectory = "./artifacts"
     };
    DotNetCoreNuGetPush("*.nupkg", settings);
});

Task("Build")
  .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Framework = "netcoreapp2.2",
        Configuration = "Debug",
        OutputDirectory = "./artifacts/"
    };

    DotNetCoreBuild("./plr.sln", settings);
});

Task("Test")
  .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = "Debug",
        OutputDirectory = "./../artifacts/"
    };

    DotNetCoreTest("./test/plr-tests.csproj", settings);
});

Task("Test-With-Coverage")
  .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = "Debug",
        ArgumentCustomization = args =>
            args.Append("/p:CollectCoverage=true")
                .Append("/p:CoverletOutputFormat=opencover")
                .Append("/p:Exclude=\"[plr]plr.BassLib*\"")
                .Append("/p:Include=[plr*]*")
    };

    DotNetCoreTest("./test/plr-tests.csproj", settings);
});

Task("Generate-Coverage")
  .Does(() =>
{
    DotNetCoreTool("./test/plr-tests.csproj", "reportgenerator", "-reports:coverage.opencover.xml -targetDir:./../artifacts/coverage");
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("Coverage")
    .IsDependentOn("Clean")
    .IsDependentOn("Test-With-Coverage")
    .IsDependentOn("Generate-Coverage");

Task("Publish")
    .IsDependentOn("Clean")
    .IsDependentOn("Pack")
    .IsDependentOn("Push");

RunTarget(target);