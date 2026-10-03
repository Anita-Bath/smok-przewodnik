{
  pkgs ? (import <nixpkgs> {}),
  ...
}:
pkgs.mkShell rec {
  name = "smok-przewodnik";

  buildInputs = with pkgs; [
    dotnet-sdk
    nodejs
    ngrok
    supabase-cli
  ];
}
