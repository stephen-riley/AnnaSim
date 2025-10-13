# Print cstrings using .cstr directive to simulated screen

# r1: message pointer
# r5: return addr
# r7: scratch

        lwi     r1 &msg
        lli     r2 1
        lli     r3 1
        lwi     r7 &print
        jalr    r7 r5
        halt

print:  lw      r7 r1 0
        beq     r7 &done
        outc    r1 
        addi    r1 r1 1
        br      &print
done:   jmp     r5

msg:    .cstr   "Hello, world!\n"
