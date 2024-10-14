# Print cstrings using .cstr directive

# Register usage:
# r1: current char addr
# r2: current screen addr
# r3: current char

        lli     r1 &msg
        lui     r1 &msg

        lli     r2 &screen
        lui     r2 &screen

loop:   lw      r3 r1 0
        beq     r3 &done
        sw      r3 r2 0
        addi    r1 r1 1
        addi    r2 r2 1
        beq     r0 &loop
        
done:   halt

msg:    .cstr   "Hello, world!"
screen: .def    0xc000