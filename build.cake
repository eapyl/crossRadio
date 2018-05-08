#addin "Cake.Putty"

var target = Argument("target", "Default");
//var runtime = Argument("runtime", "win-x64");
var runtime = Argument("runtime", "linux-arm");

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

Task("Deploy")
.WithCriteria(runtime == "linux-arm")
.Does(() => {
    Pscp("./artifacts/linux-arm/*", "192.168.1.18:/home/pi/radio", new PscpSettings{ Password = "raspberry", User = "pi" });
});

Task("Run")
.IsDependentOn("Deploy")
.WithCriteria(runtime == "linux-arm")
.Does(() => {
    Plink("192.168.1.18", "cd /home/pi/radio; nohup ./rsRadio &", new PlinkSettings { Password = "raspberry", User = "pi" });
});

Task("Default")
.IsDependentOn("Copy-Bass")
.IsDependentOn("Run");

RunTarget(target);