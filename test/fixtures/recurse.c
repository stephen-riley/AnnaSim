int recurse(int n);

recurse(1);

int recurse(int n) {
    printn(n); println();
    if (n>0) {
        return recurse(n-1);
    } else {
        return n;
    }
}
