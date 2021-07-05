namespace QuakeSquareRootTester
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    class Program
    {
        [DllImport("QuakeSquareRootLib", EntryPoint = "Q_rsqrt")]
        public static extern float Q_rsqrt(float number);

        private class Result
        {
            public float Input { get; set; }

            public float UnmanagedResult { get; set; }
            public double UnmanagedResultTimeTaken { get; set; }
            public float UnmanagedErrorPrecentage { get; set; }

            public float UnsafeResult { get; set; }
            public double UnsafeResultTimeTaken { get; set; }
            public float UnsafeErrorPrecentage { get; set; }

            public float NativeResult { get; set; }
            public double NativeResultTimeTaken { get; set; }
        }

        unsafe static float SquareRootFloat(float number)
        {
            long i;
            float x, y;
            const float f = 1.5F;

            x = number * 0.5F;
            y = number;
            i = *(long*)&y;
            i = 0x5f3759df - (i >> 1);
            y = *(float*)&i;
            y = y * (f - (x * y * y));
            //y = y * (f - (x * y * y));

            return number * y;
        }

        static void Main(string[] args)
        {
            int Cycles = 10000000;
            Random rnd = new Random();
            var results = new List<Result>();
            TimeSpan cycleTestTime;
            DateTime cycleStart, cycleEnd; 

            Console.WriteLine("Generating test data");
            // Generate test numbers
            for (int i = 0; i < Cycles; i++)
            {
                results.Add(new Result
                {
                    Input = (float)(rnd.NextDouble() * 10000.0)
                });
            }
            Console.WriteLine("Generated test data");

            // Native Test
            Console.WriteLine("Running Native Test");
            cycleStart = DateTime.Now;
            foreach (Result result in results)
            {
                TimeSpan SqrtTime;

                DateTime Start = DateTime.Now;
                var testResult = (float)Math.Sqrt((float)result.Input);
                DateTime End = DateTime.Now;

                SqrtTime = End - Start;
                result.NativeResult = testResult;
                result.NativeResultTimeTaken = SqrtTime.TotalMilliseconds;
            }
            cycleEnd = DateTime.Now;
            cycleTestTime = cycleEnd - cycleStart;
            Console.WriteLine("Finished Native Test in {0} ms", cycleTestTime.TotalMilliseconds);

            // Unsafe Test
            Console.WriteLine("Running Unsafe Test");
            cycleStart = DateTime.Now;
            foreach (Result result in results)
            {
                TimeSpan SqrtTime;

                DateTime Start = DateTime.Now;
                var testResult = SquareRootFloat(result.Input);
                DateTime End = DateTime.Now;

                SqrtTime = End - Start;
                result.UnsafeResult = testResult;
                result.UnsafeResultTimeTaken = SqrtTime.TotalMilliseconds;
                result.UnsafeErrorPrecentage = Math.Abs((testResult - result.NativeResult) / result.NativeResult * 100);
            }
            cycleEnd = DateTime.Now;
            cycleTestTime = cycleEnd - cycleStart;
            Console.WriteLine("Finished Unsafe Test in {0} ms", cycleTestTime.TotalMilliseconds);

            // Unmanaged Test
            Console.WriteLine("Running Unmanaged Test");
            cycleStart = DateTime.Now;
            foreach (Result result in results)
            {
                TimeSpan SqrtTime;

                DateTime Start = DateTime.Now;
                var testResult = Q_rsqrt(result.Input);
                DateTime End = DateTime.Now;

                SqrtTime = End - Start;
                result.UnmanagedResult = testResult;
                result.UnmanagedResultTimeTaken = SqrtTime.TotalMilliseconds;
                result.UnmanagedErrorPrecentage = Math.Abs((testResult - result.NativeResult) / result.NativeResult * 100);
            }
            cycleEnd = DateTime.Now;
            cycleTestTime = cycleEnd - cycleStart;
            Console.WriteLine("Finished Unmanaged Test in {0} ms", cycleTestTime.TotalMilliseconds);

            // Average result times
            var averageNativeTimeTaken = results.Average(x => x.NativeResultTimeTaken);
            var averageUnsafeTimeTaken = results.Average(x => x.UnsafeResultTimeTaken);
            var averageUnmanagedTimeTaken = results.Average(x => x.UnmanagedResultTimeTaken);

            LogResult(LogType.Average, ResultType.Native, averageNativeTimeTaken);
            LogResult(LogType.Average, ResultType.Unsafe, averageUnsafeTimeTaken);
            LogResult(LogType.Average, ResultType.Unmanaged, averageUnmanagedTimeTaken);

            // Highest Result Times 
            var maxNativeTimeTaken = results.Max(x => x.NativeResultTimeTaken);
            var maxUnsafeTimeTaken = results.Max(x => x.UnsafeResultTimeTaken);
            var maxUnmanagedTimeTaken = results.Max(x => x.UnmanagedResultTimeTaken);

            LogResult(LogType.High, ResultType.Native, maxNativeTimeTaken);
            LogResult(LogType.High, ResultType.Unsafe, maxUnsafeTimeTaken);
            LogResult(LogType.High, ResultType.Unmanaged, maxUnmanagedTimeTaken);

            // Min Result Times 
            var minNativeTimeTaken = results.Min(x => x.NativeResultTimeTaken);
            var minUnsafeTimeTaken = results.Min(x => x.UnsafeResultTimeTaken);
            var minUnmanagedTimeTaken = results.Min(x => x.UnmanagedResultTimeTaken);

            LogResult(LogType.Min, ResultType.Native, minNativeTimeTaken);
            LogResult(LogType.Min, ResultType.Unsafe, minUnsafeTimeTaken);
            LogResult(LogType.Min, ResultType.Unmanaged, minUnmanagedTimeTaken);

            // Error from native
            var averageUnsafeError = results.Average(x => x.UnsafeErrorPrecentage);
            var minUnsafeError = results.Min(x => x.UnsafeErrorPrecentage);
            var maxUnsafeError = results.Max(x => x.UnsafeErrorPrecentage);

            LogResult(LogType.Average, ResultType.Unsafe, averageUnsafeError, true);
            LogResult(LogType.Min, ResultType.Unsafe, minUnsafeError, true);
            LogResult(LogType.High, ResultType.Unsafe, maxUnsafeError, true);

            var averageUnmanagedError = results.Average(x => x.UnmanagedErrorPrecentage);
            var minUnmanagedError = results.Min(x => x.UnmanagedErrorPrecentage);
            var maxUnmanagedError = results.Max(x => x.UnmanagedErrorPrecentage);

            LogResult(LogType.Average, ResultType.Unmanaged, averageUnmanagedError, true);
            LogResult(LogType.Min, ResultType.Unmanaged, minUnmanagedError, true);
            LogResult(LogType.High, ResultType.Unmanaged, maxUnmanagedError, true);

            Console.Read();
        }

        private enum LogType
        {
            Average, 
            Min,
            High
        }

        private enum ResultType
        {
            Native,
            Unmanaged,
            Unsafe
        }

        private static void LogResult(LogType logType, ResultType resultType, float value, bool isError = false)
        {
            var text = isError ? "Accuracy" : "time taken";
            var valueString = isError ? $"{value}%" : $"{value}ms";
            Console.WriteLine($"[{resultType.ToString()}] [{logType.ToString()}] {text} {valueString}");
        }

        private static void LogResult(LogType logType, ResultType resultType, double value, bool isError = false)
        {
            var text = isError ? "Error from native" : "time taken";
            var valueString = isError ? $"{value}%" : $"{value}ms";
            Console.WriteLine($"[{resultType.ToString()}] [{logType.ToString()}] {text} {valueString}");
        }
    }
}
