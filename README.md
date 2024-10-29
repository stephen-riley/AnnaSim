# AnnaSim

See paper at http://fac-staff.seattleu.edu/elarson/web/Research/fie08.pdf

## Installation

AnnaSim is installed as a `dotnet tool`.

**If you are using macOS, Linux, or WSL on Windows, you can run this script to uninstall and reinstall:**

```bash
./install.sh
```

### To install:

(Note: if you have previously installed AnnaSim, you must first uninstall it.  See below.)
1. Download this repo using the GitHub Desktop app or via command line: `git clone https://github.com/stephen-riley/AnnaSim`
1. From the command line, `cd` into this directory; eg. `cd AnnaSim`
1. If you have previously installed AnnaSim: `dotnet tool uninstall --global AnnaSim`
1. Build and install AnnaSim: `dotnet pack && dotnet tool install --global --add-source src/nupkg AnnaSim`

In one copy-paste:

```bash
git clone https://github.com/stephen-riley/AnnaSim
cd AnnaSim
dotnet tool uninstall --global AnnaSim > /dev/null 2>&1
dotnet pack && dotnet tool install --global --add-source src/nupkg AnnaSim
```

### To uninstall:

```bash
dotnet tool uninstall --global AnnaSim
```

## Usage

The root command is `annasim`.  

To see command line options and switches:

```bash
annasim --help
```

```
annasim 1.0.6.28
  ANNA+ assembler and simulator

Copyright (c) 2024, Stephen Riley

USAGE:
Run an assembly file with two inputs:
  annasim -r -i 3 0x0005 myprogram.asm
Run a memory image, dumping the simulator screen after:
  annasim -r --screendump image.mem
Run an assembly file from STDIN (must pipe in the asm file):
  annasim -i 3 0x0005
Trace an assembly file with one input:
  annasim -rt -i 3 0x0005 myprogram.asm

  --disasm                  Save disassembly file
  -r, --run                 Run program instead of saving output
  -i, --input               Specifiy inputs for `in` instruction
  -d, --debug               Debug program after assembling
  -g, --optimize            (Default: 2) Optimization level (0=none, 1=opt w/
                            comments, 2=optimize)
  -a, --advanced-dbg        Debug program after assembling with VT100 debugger
  --save-asm                Save assembly file after C compile
  -c, --cc                  Compile C source
  -m, --memory-image        Save memory image
  -t, --trace               Trace output
  -y, --max-cycles          (Default: 10000) Max cycles allowed to run program
  --screendump              Dump the logical screen after execution
  --help                    Display this help screen.
  --version                 Display version information.
  InputFilename (pos. 0)    Input filename (if not specified, uses STDIN)

For more information, see
github.com/stephen-riley/AnnaSim/blob/main/docs/ANNA_Guide.pdf
```

## Docs

* [ANNA Guide](docs/ANNA_Guide.pdf)
* [ANNA Card](docs/ANNA_Card.pdf)

## Notable changes from ANNA 2.0

* Supports `mul`tiplication, `div`ision, and `mod`ulo
* Introduces programmer quality of life *pseudo-ops* such as `push`, `pop`, `lwi`, and others
* Adds the `.cstr` directive for easily declaring strings
* Adds `.org` directive
* Adds `.def` directive
* Adds `.ralias` directive to declare aliases for registers (eg. `rSP` to refer to `r7`)
* Supports block comments (`/* ... */`) -- see [ANNA Guide](docs/ANNA_Guide.pdf) for grammar

## TODO

See [TODO.md](TODO.md)
