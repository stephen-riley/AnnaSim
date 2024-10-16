#! /usr/bin/env bash

function is_on_path() {
    command -v "$1" > /dev/null 2>&1
}

if is_on_path "annasim"; then
    dotnet tool uninstall --global AnnaSim
fi

dotnet pack && dotnet tool install --global --add-source src/nupkg AnnaSim