int fib(int n);

int i = 2;
int res = fib(i);

println(); printn(i); print(": "); printn(res); println(); println();

int fib(int n) {
	print("  enter fib("); printn(n); print(")"); println();

	if (n <= 0) {
		return 0;
	} else if( n == 1 ) {
		return 1;
	} else {
		int res = fib(n-2) + fib(n-1);
		print("  exit fib("); printn(n); print("), returned "); printn(res); println();

		// int a = fib(n-2);
		// int b = fib(n-1);
		// int res = a + b;

		// print("  exit fib("); printn(n); print("): a=");
		// printn(a);
		// print(", b="); 
		// printn(b); 

		// print(", returned "); printn(res); println();
		return res;
	}
}