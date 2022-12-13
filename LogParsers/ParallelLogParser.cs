using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LogParsing.LogParsers
{
    public class ParallelLogParser : ILogParser
    {
        private readonly FileInfo file;
        private readonly Func<string, string?> tryGetIdFromLine;
        
        public ParallelLogParser(FileInfo file, Func<string, string?> tryGetIdFromLine)
        {
            this.file = file;
            this.tryGetIdFromLine = tryGetIdFromLine;
        }
        
        public string[] GetRequestedIdsFromLogFile()
        {
            var lines = File.ReadLines(file.FullName).ToArray();
            var results = new ConcurrentStack<string>();

            Parallel.For(0, lines.Length, (i) =>
            {
                var id = tryGetIdFromLine(lines[i]);
                if (id != null)
                {
                    results.Push(id);   
                }
            });

            return results.ToArray();
        }
    }
}