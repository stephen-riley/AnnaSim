int a(int n);
int b(int n);

printn(a(1));
println();

int a(int n) {
    return b(n);
}

int b(int n) {
    return 10;
}