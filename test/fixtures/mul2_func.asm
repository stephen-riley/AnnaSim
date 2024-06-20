        # SP = 0x8000
init:   lli     r7 0x8000
        lui     r7 0x8000

# mul2(n) stack frame is size 3
#       n
# FP    orig frame pointer
#       ret addr
# SP    ...

main:   in      r1
        sw      r1 r7 0
        addi    r7 r7 -1
        lli     r3 &mul2
        lui     r3 &mul2
        jalr    r3 r5
        out     r4
        .halt

# mul2(n)
        # prologue
mul2:   sw      r6 r7 +0    # store orig FP at SP+0
        addi    r6 r7 0     # set FP = SP
        sw      r5 r6 -1    # set FP-1 = return addr
        addi    r7 r7 -2    # bump SP by 3-1

        # mul2 body
        lw      r1 r6 +1
        add     r4 r1 r1

        # epilogue
epi:    lw      r5 r6 -1    # load return addr from FP-1
        lw      r6 r6 0     # restore orig FP
        addi    r7 r7 3     # SP += 3
        jalr    r5 r0       # ret
