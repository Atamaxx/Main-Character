using UnityEngine;
using System.Diagnostics;
using System;

public class PerformanceTest : MonoBehaviour
{
    private void Start()
    {
        TestPerformance();
    }

    private void TestPerformance()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        Function1();

        stopwatch.Stop();

        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        long elapsedTicks = stopwatch.ElapsedTicks;

        print($"Elapsed milliseconds: {elapsedMilliseconds}");
        print($"Elapsed ticks: {elapsedTicks}");
    }


    private void Function1()
    {
        for (int i = 0; i < 1000000; i++)
        {
            // Do nothing
        }
    }
}
