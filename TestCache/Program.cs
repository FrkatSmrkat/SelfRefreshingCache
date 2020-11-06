using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using SelfRefreshingCache;
using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace TestCache
{
    class Program
    {

        static void Main(string[] args)
        {
            var tester = new Tester();

            tester.Test().GetAwaiter().GetResult();

            return;
        }
    }


}
