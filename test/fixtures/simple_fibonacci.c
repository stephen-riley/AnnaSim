int fib(int n);

int res = fib(5);
out(res);

int fib(int n) {
	if (n<=1) {
		return n;
	}

	int a = fib(n-1);
	int b = fib(n-2);
    
	return a + b;
}