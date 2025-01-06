## TODO

## General

- [X] ...or dotnet tool?
- [X] add URL to reference docs in --help
- ~~[ ] switch to $ for hex?~~ just remove 0x in advanced debugger
- [ ] build annalib.asm
- ~~[ ] Prebuilt single-file executables?~~
- [ ] add interrupt support, eg. `syscall` (add function code to `in`)
- [ ] add `out` variant to put char to screen
- [ ] fix order of `push` and `pop` operands (SP should be Rs1, not Rd) (breaking change!)

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
- [X] allow labels on their own line
- [ ] .include directive
- [ ] Turn `.line` comments into an actual directive

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
- [X] add in() intrinsic
- [X] postfix increment/decrement
- [X] for loop
- [X] optimize `l*i rX ...` followed by `mov rY rX` to just `l*i rY ...`
- [X] optimize back to back sw-lw to same var
- [X] add deref operator
- [X] put C code line numbers in asm (new directive?)
- [X] put the line of code from C into asm comments (not pretty, but it's there)
- [ ] add array support
- [ ] #include in C
- [ ] link (really include) .mem files
- ~~[ ] don't constantly load the same value into a register if that register hasn't changed (see Note 3, below)~~ Better handle this when ported to an SSA optimizer

### Note 3

In this case, we don't need to load `&_var_a` into `r1` the second and third times
since r1 hasn't changed at all since the first assignment.  Note that any branching
instructions in between would negate this optimization.

```
lli     r3 2                # load constant 2 -> r3
mov     r2 r3               # transfer r3 to r2
lwi     r1 &_var_a          # load address of variable a
lw      r3 r1 0             # load variable "a" from data segment
add     r3 r3 r2            # execute +=
lwi     r1 &_var_a          # load address of variable "a"
sw      r3 r1 0             # store variable "a" to data segment
lwi     r1 &_var_a          # load address of variable "a"
sw      r3 r1 0             # store variable "a" to data segment
```
