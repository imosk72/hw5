using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LogParsing.LogParsers
{
    public class ThreadLogParser : ILogParser
    {
        private readonly FileInfo file;
        private readonly Func<string, string?> tryGetIdFromLine;
        
        public ThreadLogParser(FileInfo file, Func<string, string?> tryGetIdFromLine)
        {
            this.file = file;
            this.tryGetIdFromLine = tryGetIdFromLine;
        }
        
        public string[] GetRequestedIdsFromLogFile()
        {
            var lines = File.ReadLines(file.FullName).ToArray();
            
            var results = new ConcurrentStack<string>();
            var threadsCount = Environment.ProcessorCount * 4;
            int linesPerThread = (lines.Length + threadsCount - 1) / threadsCount;
            var threads = new List<Thread>();
            
            for (int i = 0; i < threadsCount; i++)
            {
                var thread = new Thread(Parse);

                int start = linesPerThread * i;
                int finish = linesPerThread * (i + 1);
                thread.Start((results, lines, start, Math.Min(finish, lines.Length)));
                threads.Add(thread);
            }
            
            foreach (var t in threads)
            {
                t.Join();
            }

            return results.ToArray();
        }
        
        private void Parse(Object obj)
        {
            var (results, lines, start, finish) = ((ConcurrentStack<string>, string[], int, int)) obj;
            for (int i = start; i < finish; i++)
            {
                var id = tryGetIdFromLine(lines[i]);
                if (id != null)
                {
                    results.Push(id);
                }
            }
        }
    }
}