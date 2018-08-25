#!/usr/bin/env bash
mkdir tools
echo "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>netcoreapp2.1</TargetFramework></PropertyGroup></Project>" > tools/tools.csproj
dotnet add tools/tools.csproj package Cake.CoreCLR -v $CAKE_VERSION --package-directory tools/Cake.CoreCLR.$CAKE_VERSION
dotnet tools/Cake.CoreCLR.$CAKE_VERSION/cake.coreclr/$CAKE_VERSION/Cake.dll build.cake -target="Default" -configuration="Release"