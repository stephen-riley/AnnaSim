## TODO

## General

- [X] ...or dotnet tool?
- [ ] switch to $ for hex?
- [ ] build annalib.asm
- [ ] Prebuilt single-file executables?
- [ ] add URL to reference docs in --help

## Assembler

- [X] dump listing with addresses
- [X] rename `outns` to `outn`
- [ ] add halt pseudo-op (but keep .halt for legacy)
- [ ] Allow + unary in asm (eg. for FP offsets)
- [ ] Expressions in assembler? like + and - (store offsets in label operands)
- [ ] .include directive
- [ ] add jmp and br pseudo ops
- [ ] add mul, div, mod instructions
- [ ] add stack frames to PDB

## Debugger

- [X] Run mode
- [X] Debug mode
- [X] Allow file load from STDIN
- [X] Allow debugging of .mem files
- [X] Add stack display to VT100 dbg
- [X] Trace mode w/ register and stack dumps
- [ ] Fix print w/out newline in V100 dbg
- [ ] Be able to label stack elements per PDB stack frames

## C Compiler

- [X] optimizer
- [ ] #include in C
- [ ] link (really include) .mem files
- [ ] for loop
- [ ] while loop
- [ ] do-while loop
- [ ] postfix increment/decrement
- [ ] add in() and out() intrinsics
