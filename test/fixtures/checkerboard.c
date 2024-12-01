int row;
int col;

char* addr = 0xc000;

for( int row=0; row<25; row = row+1 ) {
    if( row % 2 == 1 ) {
        addr--;
    } else {
        addr++;
    }

    for( int col=0; col<40; col = col+1 ) {
        *addr = 'X';
        addr += 2;
    }
}