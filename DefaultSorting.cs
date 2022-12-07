using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NameSort
{
    class name
    {
        public name(string fname, string lname)
        {
            this.firstName = fname; this.lastName = lname;
        }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }

    public class Program
    {
        public static IEnumerable<string> RunSorting(string filePath)
        {
            List<name> Names = new List<name>();
            // populate the list of names from a file
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (sr.Peek() >= 0)
                {
                    string[] s = sr.ReadLine().Split(' ');
                    var firstName = s[0];
                    string? lastName = null;
                    if (s.Length > 1)
                    {
                        lastName = s[1];
                    }

                    Names.Add(new name(firstName, lastName));
                }
            }
            // time the sort.
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<name> sortedNames = Names.OrderBy(s => s.lastName).ThenBy(s => s.firstName).ToList();
            stopwatch.Stop();
            Console.WriteLine($"Default LINQ Sort Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");

            return sortedNames.Select(n => n.firstName + " " + n?.lastName);
        }
    }
}