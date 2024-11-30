int fib(int n);

int i = in();
int res = fib(i);

out(res);

int fib(int n) {
	if (n <= 0) {
		return 0;
	} else if( n == 1 ) {
		return 1;
	} else {
		return fib(n-2) + fib(n-1);
	}
}