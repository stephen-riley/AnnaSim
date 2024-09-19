# compiled from ../test/fixtures/simple_fibonacci.c

# Register map:
#   r1  scratch register for addresses
#   r2  scratch register for temps
#   r3  scratch register for expressions results
#   r4  result from function calls
#   r5  function call destinations (jalr)
#   r6  current frame pointer (FP)
#   r7  stack pointer (SP)

# .text segment

              .org    0x0000              

              # set up main() stack frame
              lwi     r7 &_stack          # initialize SP (r7)
              add     r6 r7 r0            # initialize FP (r6)

# start of main
              lli     r3 2                # load constant 2 -> r3
              lwi     r1 &_var_i          # load address of variable "i"
              sw      r3 r1 0             # store variable "i" to data segment

              lwi     r1 &_var_i          # load address of variable i
              lw      r3 r1 0             # load variable "i" from data segment
              push    r7 r3               # push result

              lwi     r1 &fib             # load address of "fib" -> r1
              jalr    r1 r5               # call function "fib"
              mov     r3 r4               # transfer r4 to r3
              lwi     r1 &_var_res        # load address of variable "res"
              sw      r3 r1 0             # store variable "res" to data segment

              lwi     r1 &__nl            # load addr of newline
              outs    r1                  # print newline

              lwi     r1 &_var_i          # load address of variable i
              lw      r3 r1 0             # load variable "i" from data segment
              outns   r3                  # print int at r3

              lwi     r3 &_cstr001        
              outs    r3                  # print string at r3

              lwi     r1 &_var_res        # load address of variable res
              lw      r3 r1 0             # load variable "res" from data segment
              outns   r3                  # print int at r3

              lwi     r1 &__nl            # load addr of newline
              outs    r1                  # print newline

              lwi     r1 &__nl            # load addr of newline
              outs    r1                  # print newline

              .halt                       # end program

# start of functions

# function `int fib(int n)`
#  FP+1  n
#  FP+0  previous SP
#  FP-1  return addr
#  FP-2  res

fib:          push    r7 r6               # cache FP
              addi    r6 r7 1             # set up new FP
              push    r7 r5               # push return address
              addi    r7 r7 -1            # create space for stack frame

fib_body:     lwi     r3 &_cstr002        
              outs    r3                  # print string at r3

              lw      r3 r6 1             # load "n" from FP+1
              outns   r3                  # print int at r3

              lwi     r3 &_cstr003        
              outs    r3                  # print string at r3

              lwi     r1 &__nl            # load addr of newline
              outs    r1                  # print newline

              # ifs00 test condition
ifs00:        lwi     r2 0                
              lw      r3 r6 1             # load "n" from FP+1
              sub     r2 r3 r2            # compare r2 and r3
              lli     r3 1                # assume true preemptively
              ble     r2 1                # jump past the next instruction if "<=" is true
              lli     r3 0                # result is false (0) otherwise
              mov     r2 r3               # transfer r3 to r2
              beq     r2 &ifx00           # condition failed, goto next condition

              # ifs00 block
ifb00:        lli     r3 0                # load constant 0 -> r3
              mov     r4 r3               # transfer r3 to r4
              beq     r0 &fib_exit        # return (jump to func exit)
              # ifx00 elseif condition
ifx00:        lwi     r2 1                
              lw      r3 r6 1             # load "n" from FP+1
              sub     r2 r3 r2            # compare r2 and r3
              lli     r3 1                # assume true preemptively
              beq     r2 1                # jump past the next instruction if "==" is true
              lli     r3 0                # result is false (0) otherwise
              beq     r3 &ifx01           # condition failed, goto next condition
              # ifx00 block
ifb01:        lli     r3 1                # load constant 1 -> r3
              mov     r4 r3               # transfer r3 to r4
              beq     r0 &fib_exit        # return (jump to func exit)
              # ifs00 else block
ifx01:
ifb02:        lwi     r2 1                
              lw      r3 r6 1             # load "n" from FP+1
              sub     r3 r3 r2            # perform "-" on r2, r3
              push    r7 r3               # push result of "n-1"

              lwi     r1 &fib             # load address of "fib" -> r1
              jalr    r1 r5               # call function "fib"
              push    r7 r4               # push fib(...)'s result

              lwi     r2 2                
              lw      r3 r6 1             # load "n" from FP+1
              sub     r3 r3 r2            # perform "-" on r2, r3
              push    r7 r3               # push result of "n-2"

              lwi     r1 &fib             # load address of "fib" -> r1
              jalr    r1 r5               # call function "fib"
              mov     r2 r4               # transfer r4 to r2
              pop     r7 r3               # pop arg1 (lhs) for op "+"
              add     r3 r3 r2            # perform "+" on r2, r3
              sw      r3 r6 -2            # store "res" to FP-2

              lwi     r3 &_cstr004        
              outs    r3                  # print string at r3

              lw      r3 r6 1             # load "n" from FP+1
              outns   r3                  # print int at r3

              lwi     r3 &_cstr005        
              outs    r3                  # print string at r3

              lw      r3 r6 -2            # load "res" from FP-2
              outns   r3                  # print int at r3

              lwi     r1 &__nl            # load addr of newline
              outs    r1                  # print newline

              lw      r3 r6 -2            # load "res" from FP-2
              mov     r4 r3               # transfer r3 to r4
ifxx00:
fib_exit:     lw      r5 r6 -1            # load return addr from FP-1
              lw      r6 r6 0             # restore previous FP
              addi    r7 r7 4             # collapse stack frame
              jalr    r5 r0               # return from function

# .data segment

_var_i:       .fill   0                   # global variable i
_var_res:     .fill   0                   # global variable res

__nl:         .cstr   "\n"                # interned string
_cstr001:     .cstr   ": "                # interned string
_cstr002:     .cstr   "  enter fib("      # interned string
_cstr003:     .cstr   ")"                 # interned string
_cstr004:     .cstr   "  exit fib("       # interned string
_cstr005:     .cstr   "), returned "      # interned string

_stack:       .def    0x8000              # stack origination
