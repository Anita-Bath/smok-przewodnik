{
  pkgs ? (import <nixpkgs> {}),
  ...
}:
pkgs.mkShell rec {
  name = "smok-przewodnik";

  buildInputs = with pkgs; [
    (dotnetCorePackages.combinePackages (with dotnetCorePackages; [
	sdk_8_0-bin
	sdk_10_0-bin
    ]))
    dotnet-ef
    csharp-ls
    nodejs
    ngrok
    supabase-cli
  ];
}
