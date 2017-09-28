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
            Console.WriteLine("Creating Fake Data");
            string[] fakedata = new string[460];
            List<int> rngints = new List<int>();
            Random rand = new Random();
            Console.WriteLine("Generating index");
            while(rngints.Count != 460){
                int guess = rand.Next(4600);
                if(!rngints.Contains(guess)) rngints.Add(guess);
            }
            string faketext = "";
            Console.WriteLine("Generating fake data");
            foreach(int x in rngints){
                faketext += lines[x].Trim() + "\n";
            }
            string realdata = "";
            Console.WriteLine("Generating real data");
            for(int i = 0; i < 4600; i++){
                if(!rngints.Contains(i)){
                    realdata += lines[i].Trim() + "\n";
                }
            }

            File.WriteAllText("FakeData.csv",faketext);
            File.WriteAllText("RealData.csv", realdata);
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
