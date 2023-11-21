using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sentry;
using Sentry.Profiling;

internal static class Program
{
    private static void Main()
    {
        // Enable the SDK
        using (SentrySdk.Init(options =>
        {
            options.Dsn =
                // NOTE: ADD YOUR OWN DSN BELOW so you can see the events in your own Sentry account
                "";

            options.Debug = true;
            // options.AutoSessionTracking = true;
            options.IsGlobalModeEnabled = true;
            options.EnableTracing = true;

            // Debugging
            options.ShutdownTimeout = TimeSpan.FromMinutes(5);

            options.AddIntegration(new ProfilingIntegration());
        }))
        {
            DoPrimeStuff();

        }  // On Dispose: SDK closed, events queued are flushed/sent to Sentry
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void DoPrimeStuff()
    {
        var tx = SentrySdk.StartTransaction("app3", "run");
        var count = 10;
        for (var i = 0; i < count; i++)
        {
            FindPrimeNumber(100000);
        }

        tx.Finish();
        var sw = Stopwatch.StartNew();

        // Flushing takes 10 seconds consistently?
        SentrySdk.Flush(TimeSpan.FromMinutes(5));
        Console.WriteLine("Flushed in " + sw.Elapsed);

        // is the second profile faster?
        tx = SentrySdk.StartTransaction("app3", "run");
        var sp = tx.StartChild("first-op");
        count = 10;
        for (var i = 0; i < count; i++)
        {
            FindPrimeNumber(100000);
        }
        sp.Finish();
        sp = tx.StartChild("second-op");
        for (var i = 0; i < count; i++)
        {
            FindPrimeNumber(100000);
        }
        sp.Finish();

        tx.Finish();
        sw = Stopwatch.StartNew();

        // Flushing takes 10 seconds consistently?
        SentrySdk.Flush(TimeSpan.FromMinutes(5));
        Console.WriteLine("Flushed in " + sw.Elapsed);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static long FindPrimeNumber(int n)
    {
        int count = 0;
        long a = 2;
        while (count < n)
        {
            long b = 2;
            int prime = 1;// to check if found a prime
            while (b * b <= a)
            {
                if (a % b == 0)
                {
                    prime = 0;
                    break;
                }
                b++;
            }
            if (prime > 0)
            {
                count++;
            }
            a++;
        }
        return (--a);
    }
}
