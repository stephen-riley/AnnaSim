int fib(int n);

int i = 6;
int res = fib(i);

printn(i); print(": "); printn(res); println();

int fib(int n) {
	print("in @"); printn(n); println();

	if (n<=1) {
		print("check "); printn(n); println();
		return n;
	}

	int res = fib(n-2) + fib(n-1);
	return res;
}