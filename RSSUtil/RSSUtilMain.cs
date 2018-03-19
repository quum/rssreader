using CommandLine;
using RSSLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace RSSUtil
{
    class Options
    {
        [Option('s', "scan", HelpText = "Scan RSS Feeds")]
        public bool Scan { get; set; }

        [Option('i', "import", HelpText = "Import OPML file")]
        public string ImportFile { get; set; }

        [Option('m', "merge", HelpText = "Merge OPML file")]
        public string MergeFile { get; set; }

        [Option('d', "database", Required =true, HelpText = "Database (schema:user:password:server:port")]
        public string Database { get; set; }
    }

    class RSSUtilMain
    {
        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => run(opts));

        }



        private static void run(Options opts)
        {
            if(!string.IsNullOrWhiteSpace(opts.MergeFile)&&!string.IsNullOrWhiteSpace(opts.ImportFile))
            {
                Console.WriteLine($"Cannot merge and import!");
                return;
            }

            if (opts.Scan)
            {
                var scanner = new FeedScanner(new Database(DatabaseContext.Parse(opts.Database)));
                scanner.Scan();
            }
            else
            {
                var file = string.Empty;
                var merge = false;
                if (!string.IsNullOrWhiteSpace(opts.ImportFile))
                {
                    if (!File.Exists(opts.ImportFile))
                    {
                        Console.WriteLine($"File not found: {opts.ImportFile}");
                        return;
                    }

                    file = opts.ImportFile;
                }
                else if (!string.IsNullOrWhiteSpace(opts.MergeFile))
                {
                    if (!File.Exists(opts.MergeFile))
                    {
                        Console.WriteLine($"File not found: {opts.MergeFile}");
                        return;
                    }
                    merge = true;
                    file = opts.MergeFile;
                }
                var loader = new OpmlLoader(new Database(DatabaseContext.Parse(opts.Database)));
                loader.LoadOpmlFile(file, merge);
            }
        }

     
        
    }
}
 