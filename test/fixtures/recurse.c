int recurse(int n);

recurse(20);

int recurse(int n) {
    printn(n); println();
    if (n>0) {
        int a = recurse(n-2);
        return a;
    } else {
        return n;
    }
}
