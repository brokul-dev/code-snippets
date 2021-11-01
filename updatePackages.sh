#!/bin/bash

(cd ./src && dotnet restore)

regex='PackageReference Include="([^"]*)" Version="([^"]*)"'

echo 'Updating nuget packages...'
for csproj in ./src/*/*.csproj; do
    csprojFullPath=$(readlink -f "$csproj")
    while read -r line; do
        if [[ $line =~ $regex ]]; then
            package="${BASH_REMATCH[1]}"
            dotnet add "$csprojFullPath" package "$package"
        fi
    done <"$csproj"
done

echo 'Updating npm packages...'
for packageJson in ./src/*/package.json ./src/*/*/package.json; do
    dirName=$(dirname "$packageJson")
    (cd "$dirName" && ncu -u --packageFile package.json && npm install)
done
