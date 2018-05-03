var target = Argument("target", "Default");
var runtime = Argument("runtime", "win-x64");
//var runtime = Argument("runtime", "linux-arm");

Task("Clean")
.Does(() => {
    CleanDirectory($"./artifacts/{runtime}");
});

// Task("Build")
// .IsDependentOn("Clean")
// .Does(() => {
//     var settings = new DotNetCoreBuildSettings
//      {
//          Framework = "netcoreapp2.0",
//          Configuration = "Debug",
//          OutputDirectory = $"./bin/{runtime}",
//          Runtime = runtime
//      };

//      DotNetCoreBuild("./rsRadio.csproj", settings);
// });

Task("Publish")
.IsDependentOn("Clean")
.Does(() => {
    var settings = new DotNetCorePublishSettings
     {
         Framework = "netcoreapp2.0",
         Configuration = "Release",
         OutputDirectory = $"./artifacts/{runtime}",
         Runtime = runtime
     };

     DotNetCorePublish("./rsRadio.csproj", settings);
});

Task("Copy-Bass")
.IsDependentOn("Publish")
.Does(() => {
    if (runtime == "win-x64")
    {
        CopyFile($"./bass/bass.dll", $"./artifacts/{runtime}/bass.dll");
    } 
    else if (runtime == "linux-arm")
    {
        CopyFile($"./bass/libbass.so", $"./artifacts/{runtime}/libbass.so");
    }
    
});

Task("Default")
.IsDependentOn("Copy-Bass");

RunTarget(target);