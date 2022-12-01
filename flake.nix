{
  description = "A very basic flake";

  outputs = { self, nixpkgs }: {

    packages.x86_64-linux.cunnyapi = with import nixpkgs { system = "x86_64-linux"; }; pkgs.callPackage ./default.nix {};

    packages.x86_64-linux.default = self.packages.x86_64-linux.cunnyapi;

  };
}
