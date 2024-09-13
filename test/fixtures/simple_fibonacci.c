int fib(int n);

int i = 6;
int res = fib(i);

printn(i); print(": "); printn(res); println();

int fib(int n) {
	print("enter fib("); printn(n); print(")"); println();
	if (n<=1) {
		print("  fib("); printn(n); print(") early returned "); printn(n); println();
		return n;
	}

	int res = fib(n-2) + fib(n-1);
	print("  fib("); printn(n); print(") returned "); printn(res); println();
	return res;
}