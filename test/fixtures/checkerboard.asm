# Render a checkerboard at 0x8000 (screen memory)
#
# char* addr = 0x83e7;
# 
# while( addr >= 0x8000 ) {
#     int mod2 = addr & 1;
#     if( mod2 == 0 ) {
#         *addr = 'X';
#     }
#     addr--;
# }
# 
# r1: addr
# r2: mod2
# r4: const 'X'
# r5: const 0x8000
# r6: tmp

        # initialize variables

        lli     r1 &scrend
        lui     r1 &scrend

        lli     r4 0x58         # 'X'
        
        lli     r5 &screen
        lui     r5 &screen

while:  sub     r6 r1 r5
        blt     r6 &end

        lli     r6 1
        and     r6 r6 r1
        bne     r6 &dec

        sw      r4 r1 0

dec:    addi    r1 r1 -1
        beq     r0 &while

end:    .halt

screen: .def    0x8000
scrend: .def    0x83e7          # 0x8000 + 999 decimal,
                                # last char on screen
