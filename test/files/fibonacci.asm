        in      r4          # input n
        add     r1 r0 r0    # a = 0
        addi    r2 r0 1     # b = 1

loop:   addi    r4 r4 -1    # decrement n
        beq     r4 &end     # branch if done
        add     r3 r1 r2    # c = a + b
        add     r1 r2 r0    # a = b
        add     r2 r3 r0    # b = c
        beq     r0 &loop

end:    out     r3          # print result
        lli     r7 0x00     # load LED addr
        lui     r7 0xC0     # load LED addr
        sw      r3 r7 0     # store in LEDs
        sw      r3 r7 1     # store in 7-segments

        .halt
