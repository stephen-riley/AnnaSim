# r1: a
# r2: b
# r3: c (contains current fib)
# r4: n (loop counter)

        in      r4          # input n
        add     r1 r0 r0    # a = 0
        addi    r2 r0 1     # b = 1
        out     r2

loop:   
        addi    r4 r4 -1    # decrement n
        beq     r4 &end     # branch if done
        add     r3 r1 r2    # c = a + b
        add     r1 r2 r0    # a = b
        add     r2 r3 r0    # b = c
        out     r3
        beq     r0 &loop

end:    
        halt
