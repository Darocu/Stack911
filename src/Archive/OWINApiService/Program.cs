using System;
using System.Threading;
using Microsoft.Owin.Hosting;
using Serilog;

namespace owinapiservice;

class Program
{
    static void Main(string[] args)
    {
        var baseAddress = "http://localhost:9000/"; // or from config

        using (WebApp.Start<Startup>(url: baseAddress))
        {
            Console.WriteLine($"Service started at {baseAddress}. Press Ctrl+C to exit...");
            var exitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                exitEvent.Set();
            };

            exitEvent.WaitOne();
        }

        Log.CloseAndFlush();
    }
}