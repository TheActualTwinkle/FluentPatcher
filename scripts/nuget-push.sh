#!/bin/bash

set -e

# Load version from .env
if [ -f .env ]; then
  source .env
elif [ -f scripts/.env ]; then
  source scripts/.env
else
  echo ".env file not found!"
  exit 1
fi

if [ -z "$VERSION" ]; then
  echo "VERSION not set in .env"
  exit 1
fi

if [ -z "$NUGET_API_KEY" ]; then
  echo "NUGET_API_KEY environment variable not set"
  exit 1
fi

# Build all projects
dotnet clean
dotnet restore
dotnet build -c Release

# Pack all projects
dotnet pack -c Release FluentPatcher.Analyzer/FluentPatcher.Analyzer.csproj --output nupkg
dotnet pack -c Release FluentPatcher.CodeFixes/FluentPatcher.CodeFixes.csproj --output nupkg
dotnet pack -c Release FluentPatcher.Generator/FluentPatcher.Generator.csproj --output nupkg
dotnet pack -c Release FluentPatcher/FluentPatcher.csproj --output nupkg

# Publish FluentPatcher.Generator package
dotnet nuget push nupkg/FluentPatcher.Generator.$VERSION.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json

echo "Done!"
