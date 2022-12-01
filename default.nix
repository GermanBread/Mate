{ lib
, stdenv
, fetchNuGet
, buildDotnetModule
, dotnet-sdk_6
, dotnet-aspnetcore_6
, pkg-config }:

buildDotnetModule rec {
  pname = "Mate";
  version = "1.1";
  src = ./.;
  nugetDeps = ./deps.nix;
  dotnet-sdk = dotnet-sdk_6;
  dotnet-runtime = dotnet-aspnetcore_6;
  executables = [ "Mate" ];
  meta = with lib; {
    homepage = "https://github.com/GermanBread/Mate";
    description = "A friendly Discord bot";
    license = licenses.gpl3;
    platforms = [ "x86_64-linux" ];
  };
}
