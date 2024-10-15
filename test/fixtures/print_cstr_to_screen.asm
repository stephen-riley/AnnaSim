# Print cstrings using .cstr directive to simulated screen

# r1: message pointer
# r2: row
# r3: col
# r4: offset into screen
# r5: return addr
# r7: scratch

screen: .def    0xc000

        lwi     r1 &msg
        lli     r2 1
        lli     r3 1
        lwi     r7 &print
        jalr    r7 r5
        halt

print:  lli     r4 40
        mul     r4 r4 r2
        add     r4 r4 r3
        lwi     r7 &screen
        add     r4 r7 r4

loop:   lw      r7 r1 0
        beq     r7 &done
        sw      r7 r4 0
        addi    r4 r4 1
        addi    r1 r1 1
        br      &loop
done:   jmp     r5

msg:    .cstr   "Hello, world!"
