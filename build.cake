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
        OutputDirectory = "./../artifacts/"
    };

    DotNetCoreBuild("./plr.sln", settings);
});

Task("Test")
  .Does(() =>
{
    StartProcess("dotnet", new ProcessSettings {
        Arguments = "test ./test/plr-tests.csproj -o ./artifacts/"
    });
});

Task("Test-With-Coverage")
  .Does(() =>
{
    StartProcess("dotnet", new ProcessSettings {
        Arguments = "test ./test/plr-tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:Exclude=\"[plr]plr.BassLib*\" -o ./../artifacts/"
    });
});

Task("Generate-Coverage")
  .Does(() =>
{
    StartProcess("dotnet", new ProcessSettings {
        Arguments = "reportgenerator -reports:coverage.opencover.xml -targetDir:./../artifacts/coverage",
        WorkingDirectory = ".\\test"
    });
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