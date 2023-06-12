namespace Fluxor.Benchmarks;

public static class Program
{
	public static void Main(string[] args)
	{
		BenchmarkDotNet.Running.BenchmarkRunner.Run(typeof(Program).Assembly);
	}
}