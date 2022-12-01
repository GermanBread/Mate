#!/usr/bin/env nix-shell
#!nix-shell -i bash -p nuget-to-nix dotnet-sdk_6

rm -rf pkgs
echo "Restoring to ./pkgs/"
dotnet restore --packages pkgs
echo "Generating deps.nix"
nuget-to-nix pkgs >deps.nix
echo "Done"