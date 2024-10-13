# AnnaSim

See paper at http://fac-staff.seattleu.edu/elarson/web/Research/fie08.pdf

## Installation

AnnaSim is installed as a `dotnet tool`.

To install:

(Note: if you have previously installed AnnaSim, you must first uninstall it.  See below.)
1. Download this repo using the GitHub Desktop app or via command line: `git clone https://github.com/stephen-riley/AnnaSim`
1. From the command line, `cd` into this directory; eg. `cd AnnaSim`
1. If you have previously installed AnnaSim: `dotnet tool uninstall --global AnnaSim`
1. Build and install AnnaSim: `dotnet pack && dotnet tool install --global --add-source src/nupkg AnnaSim`

To uninstall:
`dotnet tool uninstall --global AnnaSim`

## Usage

The root command is `anna`.  

Type `annasim --help` for a list of subcommands.

To get help for a given subcommand, such as `debug`, type `annasim debug --help`.

## Docs

* [ANNA Guide](docs/ANNA_Guide.pdf)
* [ANNA Card](docs/ANNA_Card.pdf)

## TODO

See [TODO.md](TODO.md)
