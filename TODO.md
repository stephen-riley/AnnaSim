## TODO

## General

- [X] ...or dotnet tool?
- [X] add URL to reference docs in --help
- [ ] ~~switch to $ for hex?~~ just remove 0x in advanced debugger
- [ ] build annalib.asm
- ~~[ ] Prebuilt single-file executables?~~
- [ ] add interrupt support (add function code to `in`)
- [ ] add `out` variant to put char to screen

## Assembler

- [X] dump listing with addresses
- [X] rename `outns` to `outn`
- [X] add halt pseudo-op (but keep .halt for legacy)
- [X] add jmp and br pseudo ops
- [X] add mul, div, mod instructions
- [X] add register aliases (not hardcoded like SP and FP)
- [X] Expressions in assembler? like + and - (store offsets in label operands)
- [X] add stack frames to PDB
- [X] test frame pointer activation -- sometimes it's not recognizing the frame region
- [ ] allow labels on their own line
- [ ] .include directive

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
- [X] do-while loop
- [X] while loop
- [X] add out() intrinsic
- [ ] #include in C
- [ ] link (really include) .mem files
- [ ] for loop
- [ ] postfix increment/decrement
- [ ] add in() intrinsic
- [ ] optimize back to back sw-lw to same var (see Note 1, below)

### Note 1

In this case, the second half (`lwi` of `&_var_b` and then the `lw`) should be
optimized away.

```
lwi     r1 &_var_b          # load address of variable "b"
sw      r3 r1 0             # store variable "b" to data segment

lwi     r1 &_var_b          # load address of variable b
lw      r3 r1 0             # load variable "b" from data segment
```

should simply become

```
lwi     r1 &_var_b          # load address of variable "b"
sw      r3 r1 0             # store variable "b" to data segment
```
