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

echo Version = $VERSION

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

NUGET_SOURCE="https://api.nuget.org/v3/index.json"

confirm_and_push() {
  local package_path="$1"

  echo "Will run:"
  echo "dotnet nuget push \"$package_path\" --api-key ***hidden*** --source \"$NUGET_SOURCE\""

  read -r -p "Push this package? [Y/n]: " CONFIRM
  if [[ "$CONFIRM" =~ ^[Nn]$ ]]; then
    echo "Skipped: $package_path"
    return
  fi

  dotnet nuget push "$package_path" --api-key "$NUGET_API_KEY" --source "$NUGET_SOURCE"
}

echo "Packages ready for publish:"
echo "- nupkg/FluentPatcher.Generator.$VERSION.nupkg"
echo "- nupkg/FluentPatcher.Analyzer.$VERSION.nupkg"
echo "- nupkg/FluentPatcher.CodeFixes.$VERSION.nupkg"
echo "- nupkg/FluentPatcher.$VERSION.nupkg"

read -r -p "Continue to publishing step? [Y/n]: " CONFIRM_ALL
if [[ "$CONFIRM_ALL" =~ ^[Nn]$ ]]; then
  echo "Publishing cancelled."
  exit 0
fi

confirm_and_push "nupkg/FluentPatcher.Generator.$VERSION.nupkg"
confirm_and_push "nupkg/FluentPatcher.Analyzer.$VERSION.nupkg"
confirm_and_push "nupkg/FluentPatcher.CodeFixes.$VERSION.nupkg"
confirm_and_push "nupkg/FluentPatcher.$VERSION.nupkg"

echo "Done!"
