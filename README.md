# AnnaSim

See paper at http://fac-staff.seattleu.edu/elarson/web/Research/fie08.pdf

## Installation

AnnaSim is installed as a `dotnet tool`.

To install:

(Note: if you have previously installed AnnaSim, you must first uninstall it.  See below.)
1. Download or pull this source code.
2. `cd` into this directory.
3. `dotnet pack`
4. `dotnet tool install --global --add-source src/nupkg AnnaSim`

To uninstall:
`dotnet tool uninstall --global AnnaSim`

## Usage

The root command is `anna`.  

Type `anna --help` for a list of subcommands.

To get help for a given subcommand, such as `debug`, type `anna debug --help`.

## Docs

* [ANNA Guide](docs/ANNA_Guide.pdf)
* [ANNA Card](docs/ANNA_Card.pdf)

## TODO

See [TODO.md](TODO.md)
