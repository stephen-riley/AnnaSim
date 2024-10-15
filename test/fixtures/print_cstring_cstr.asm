# Print cstrings using .cstr directive

# Register usage:
# r1: current char addr
# r2: current screen addr
# r3: current char

screen: .def    0xc000

        lwi     r1 &msg
        lwi     r2 &screen

loop:   lw      r3 r1 0
        beq     r3 &done
        sw      r3 r2 0
        addi    r1 r1 1
        addi    r2 r2 1
        beq     r0 &loop
        
done:   halt

msg:    .cstr   "Hello, world!"
