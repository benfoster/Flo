using System;
using System.Linq;
using System.Reflection;
using NSpec;
using NSpec.Domain;
using NSpec.Domain.Formatters;

namespace Flo.Tests
{
    public class Program
    {
        static void Main(string[] args)
        {
            var tags = args.FirstOrDefault() ?? "";
            
            var types = Assembly.GetEntryAssembly().GetTypes();
            var finder = new SpecFinder(types, "");
            var tagsFilter = new Tags().Parse(tags);
            var builder = new ContextBuilder(finder, tagsFilter, new DefaultConventions());
            var runner = new ContextRunner(tagsFilter, new ConsoleFormatter(), false);
            var results = runner.Run(builder.Contexts().Build());

            if(results.Failures().Count() > 0)
            {
                Environment.Exit(1);
            }
        }
    }
}