using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines("all-data.csv");
            string[][] data = new string[lines.Length][];
            for(int i = 0; i < lines.Length; i++){
                data[i] = new string[]{lines[i].Split(',')[0].Trim(), lines[i].Split(',')[1].Trim()};
            }
            
            Console.ReadLine();
        }
    }
}
