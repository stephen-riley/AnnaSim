/*
    Render a checkerboard at 0x8000 (screen memory)

    int row;
    int col;
    int addr = 0xc000;

    for( row=0; row<25; row++ ) {
        if( row % 2 == 1 ) {
            addr--;
        } else {
            addr++;
        }

        for( col=0; col<40; col++ ) {
            *addr = 'X';
            addr = addr + 2;
        }
    }
*/

# r1: addr
# r4: const 'X'
# rScr: const &screen (start of screen map)
# rTmp: tmp

.ralias rAddr   r1
.ralias rX      r4
.ralias rScr    r5
.ralias rTmp    r6

screen: .def    0xc000
scrend: .def    0xc3e7

        # initialize variables

        lwi     rAddr &scrend

        lli     rX 0x58         # 'X'

        lwi     rScr &screen

while:  sub     rTmp rAddr rScr
        blt     rTmp &end

        lli     rTmp 1
        and     rTmp rTmp rAddr
        bne     rTmp &dec

        sw      rX rAddr 0

dec:    addi    rAddr rAddr-1
        beq     r0 &while

end:    halt

screen: .def    0xc000
scrend: .def    0xc3e7          # 0xc000 + 999 decimal,
                                # last char on screen
