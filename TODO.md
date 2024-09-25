## TODO

## General

- [ ] switch to $ for hex?
- [ ] build annalib.asm
- [ ] Prebuilt single-file executables?
- [ ] ...or dotnet tool?

## Assembler

- [ ] Allow + unary in asm (eg. for FP offsets)
- [ ] Expressions in assembler?
- [ ] .include directive
- [ ] add jmp/br pseudo op
- [X] dump listing with addresses
- [ ] add mul, div, mod instructions
- [ ] add stack frames to PDB

## Debugger

- [X] Run mode
- [X] Debug mode
- [X] Allow file load from STDIN
- [X] Allow debugging of .mem files
- [X] Add stack display to VT100 dbg
- [ ] Fix print w/out newline in V100 dbg
- [ ] Be able to label stack elements per PDB stack frames

## C Compiler

- [ ] #include in C
- [ ] link (really include) .mem files
- [ ] for loop
- [ ] while loop
- [ ] do-while loop
- [ ] postfix increment/decrement
- [X] optimizer
