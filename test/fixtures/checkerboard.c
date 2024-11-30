int row;
int col;

char* addr = 32768;

for( int row=0; row<25; row = row+1 ) {
    if( row % 2 == 1 ) {
        addr--;
    } else {
        addr++;
    }

    for( int col=0; col<40; col = col+1 ) {
        *addr = 'X';
        addr = addr + 2;
    }
}