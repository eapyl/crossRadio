// publish to NuGet
// dotnet pack --configuration Release
// dotnet nuget push .\bin\release\plr.1.0.2.nupkg --source https://api.nuget.org/v3/index.json --api-key {key}

// local run
// Start-Process -FilePath 'dotnet' -ArgumentList 'run --debug'

var target = Argument("target", "Default");

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./artifacts/");
});

Task("Build")
  .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Framework = "netcoreapp2.1",
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
        OutputDirectory = "./../artifacts/",
        ArgumentCustomization = args =>
            args.Append("/p:CollectCoverage=true")
                .Append("/p:CoverletOutputFormat=opencover")
                .Append("/p:Exclude=\"[plr]plr.BassLib*\"")
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
    .IsDependentOn("Build")
    .IsDependentOn("Test-With-Coverage")
    .IsDependentOn("Generate-Coverage");

RunTarget(target);