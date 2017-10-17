using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace DataGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            int response = 0;
        LGetResponse:
            string choices = "Actions:\n1. Generate New Fake and Real Data\n2. Generate Encrypted Data\n3. Decrypt Data\n4. Linear Test\n5. Quadratic Test";
            Console.WriteLine(choices);
            Int32.TryParse(Prompt("Choose an action.(EX: 1)"), out response);
            switch (response)
            {
                case -1:
                    break;
                case 1:
                    GenerateData(Prompt("What is the file for the data"), Double.Parse(Prompt("What percentage of data is fake?(.1 or .25 or ..)")));
                    break;
                case 2:
                    GenerateEncryptedData();
                    break;
                case 3:
                    DecryptData();
                    break;
                case 4:
                    SpeedTest();
                    break;
                case 5:
                    Quadratic();
                    Console.WriteLine("Done");
                    break;
                case 6:
                    int seed = Int32.Parse(Prompt("Pick a seed:"));
                    Random rr = new Random(seed);
                    Console.WriteLine(rr.Next(10000) + "\n" + rr.Next(10000) + "\n" + rr.Next(10000));
                    rr = new Random(seed);
                    Console.WriteLine("-----------");
                    Console.WriteLine(rr.Next(10000) + "\n" + rr.Next(10000) + "\n" + rr.Next(10000));
                    break;
                case 7:
                    RandomIndexes();
                    break;
                default:
                    Console.WriteLine("Invalid Choice");
                    goto LGetResponse;
            }
            Console.WriteLine("FINISHED");
            Console.ReadLine();
        }

        static string Prompt(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        static void DeleteFile(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        static void GenerateData(string filepath, double percent)
        {
            string[] lines = File.ReadAllLines(filepath);
            DeleteFile("FakeData.txt");
            DeleteFile("RealData.txt");
            Console.WriteLine("Creating Fake Data");
            string[] fakedata = new string[(int)(lines.Length * percent)];
            List<string> fake = new List<String>();
            List<string> real = new List<String>(lines);
            List<int> rngints = new List<int>();
            Random rand = new Random();
            Console.WriteLine("Generating index");
            while (rngints.Count != (int)(lines.Length * percent))
            {
                int guess = rand.Next(lines.Length);
                if (!rngints.Contains(guess))
                {
                    rngints.Add(guess);
                    real.Remove(lines[guess]);
                    fake.Add(lines[guess]);
                }
            }
            File.WriteAllLines("FakeData.txt", fake.ToArray());
            File.WriteAllLines("RealData.txt", real.ToArray());
            Console.WriteLine("Done");
        }

        static void GenerateEncryptedData()
        {
            byte[] content = File.ReadAllBytes("RealData.txt");
            DeleteFile("EncryptedRealData.txt");
            DeleteFile("key.txt");
            DeleteFile("iv.txt");
            File.WriteAllBytes("EncryptedRealData.txt", Encrypt(content, CipherMode.CBC));
        }

        static void DecryptData()
        {
            byte[] content = File.ReadAllBytes("EncryptedRealData.txt");
            byte[] key = File.ReadAllBytes("key.txt");
            byte[] iv = File.ReadAllBytes("iv.txt");
            DeleteFile("DecryptedRealData.txt");
            File.WriteAllBytes("DecryptedRealData.txt", Decrypt(content, key, iv, CipherMode.CBC));
        }

        public static byte[] Encrypt(byte[] input, CipherMode cipherMode)
        {
            Console.WriteLine("Encrypting Data");
            Stopwatch watch = new Stopwatch();
            byte[] val;
            watch.Start();
            using (RijndaelManaged myRijndael = new RijndaelManaged { Mode = cipherMode })
            {

                myRijndael.GenerateKey();
                myRijndael.GenerateIV();

                File.WriteAllBytes("key.txt", myRijndael.Key);
                File.WriteAllBytes("iv.txt", myRijndael.IV);
                ICryptoTransform encryptor = myRijndael.CreateEncryptor(myRijndael.Key, myRijndael.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(input, 0, input.Length);
                        csEncrypt.FlushFinalBlock();
                        val = msEncrypt.ToArray();
                    }
                }
                watch.Stop();
                // DeleteFile("Info.txt");
                File.AppendAllText("Info.txt", "\nTime for Encryption: " + watch.ElapsedMilliseconds + "ms");
                Console.WriteLine("Finished Encrypting");
                return val;
            }
        }

        public static byte[] Decrypt(byte[] input, byte[] key, byte[] iv, CipherMode cipherMode)
        {
            Console.WriteLine("Decrypting Data");
            Stopwatch watch = new Stopwatch();
            byte[] val;
            watch.Start();
            using (RijndaelManaged myRijndael = new RijndaelManaged { Mode = cipherMode })
            {

                myRijndael.IV = iv;
                myRijndael.Key = key;
                ICryptoTransform encryptor = myRijndael.CreateDecryptor(myRijndael.Key, myRijndael.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(input, 0, input.Length);
                        csEncrypt.FlushFinalBlock();
                        val = msEncrypt.ToArray();
                    }
                }
            }
            watch.Stop();
            File.AppendAllText("Info.txt", "\nTime for Decryption: " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine("Finished Decrypting");
            return val;
        }

        public static void SpeedTest()
        {
            int variation = 10;
            string[] total = File.ReadAllLines("AllData.txt");
            string[] fakes = File.ReadAllLines("FakeData.txt");
            string[] reals = File.ReadAllLines("RealData.txt");
            string[] corrected = new string[total.Length];
            Stopwatch watch = new Stopwatch();
            Console.WriteLine("Building total array");
            watch.Start();
            int f = 0;
            int r = 0;
            for (int i = 0; i < total.Length / variation; i++)
            {
                corrected[variation * i] = fakes[f];
                f++;
                for (int j = 1; j < variation; j++)
                {
                    corrected[variation * i + j] = reals[r];
                    r++;
                }
            }
            if (corrected.Length != total.Length) Console.WriteLine("ERROR NOT RIGHT BRO");
            watch.Stop();
            File.AppendAllText("Info.txt", "\nTime for Linear Encryption: " + watch.ElapsedMilliseconds + "ms");
            Stopwatch swatch = new Stopwatch();
            List<string> realdata = new List<string>();
            string[] real = new string[reals.Length];
            swatch.Start();
            int c = 0;
            for (int i = 0; i < corrected.Length; i++)
            {
                if (i % variation != 0)
                {
                    real[c] = corrected[i];
                    c++;
                }
            }
            swatch.Stop();
            File.AppendAllText("Info.txt", "\nTime for Linear Decryption: " + swatch.ElapsedMilliseconds + "ms");
            File.WriteAllLines("LinearReals.txt", real);
            Console.WriteLine("Finished");
        }

        public static int SafeIndex(int[] indexes, int index, int max)
        {
            if (indexes.Contains(index))
            {
                return SafeIndex(indexes, (index + 1) % max, max);
            }
            return index;
        }

        public static void Quadratic()
        {
            string[] total = File.ReadAllLines("AllData.txt");
            string[] fakes = File.ReadAllLines("FakeData.txt");
            string[] reals = File.ReadAllLines("RealData.txt");
            string[] corrected = new string[total.Length];
            int[] indexes = new int[fakes.Length];
            Stopwatch eWatch = new Stopwatch();
            int seed = Int32.Parse(Prompt("What seed do you want? (32 bit int only)"));
            eWatch.Start();
            // Console.Write(indexes[1]);
            Random rand = new Random(seed);
            QuadFunction fun = new QuadFunction(rand.Next(1000), rand.Next(1000), rand.Next(1000));
            // Conso/le.WriteLine("Fakes: " + fakes.Length + "\nIndexes:" + indexes.Length);
            for (int i = 0; i < fakes.Length; i++)
            {
                int index = SafeIndex(indexes, Math.Abs(fun.Evaluate(i * seed) % total.Length), total.Length);
                // Console.WriteLine(index);
                corrected[index] = fakes[i];
                // Console.WriteLine(i);
                indexes[i] = index;
            }
            int r = 0;
            for (int i = 0; i < total.Length; i++)
            {
                if (r >= reals.Length)
                {
                    Console.Write("ERRORORRORORORORO\n");
                    break;
                }
                if (!indexes.Contains(i))
                {
                    corrected[i] = reals[r];
                    r++;
                }
            }
            eWatch.Stop();
            File.AppendAllText("Info.txt", "\nTime for Quad Encryption: " + eWatch.ElapsedMilliseconds + "ms");
            bool dupsFound = false;

            for (int i = 0; i < total.Length; i++)
            {
                if (corrected[i] == null)
                {
                    dupsFound = true;
                    break;
                }
                if (corrected[i].Trim().Equals(""))
                {
                    dupsFound = true;
                    break;
                }
            }
            if (dupsFound) Console.WriteLine("FOUND DUPLICATES");
            Stopwatch dWatch = new Stopwatch();
            dWatch.Start();
            string[] eReals = new string[reals.Length];
            for (int i = 0; i < fakes.Length; i++)
            {
                int index = SafeIndex(indexes, Math.Abs(fun.Evaluate(i * seed) % total.Length), total.Length);
                corrected[index] = "";
            }
            int rr = 0;

            for (int i = 0; i < total.Length; i++)
            {
                if (rr >= eReals.Length) break;
                if (corrected[i] != null && !corrected[i].Trim().Equals(""))
                {
                    eReals[rr] = corrected[i];
                    rr++;
                }
            }
            dWatch.Stop();
            File.AppendAllText("Info.txt", "\nTime for Quad Decryption: " + dWatch.ElapsedMilliseconds + "ms");
            File.WriteAllLines("QuadReals.txt", eReals);
            Console.WriteLine("Finished");
        }

        public static void Shifty()
        {

        }

        public static void RandomIndexes()
        {
            int seed = Int32.Parse(Prompt("Pick a seed:"));

            string[] total = File.ReadAllLines("AllData.txt");
            string[] fakes = File.ReadAllLines("FakeData.txt");
            string[] reals = File.ReadAllLines("RealData.txt");
            Stopwatch eWatch = new Stopwatch();
            eWatch.Start();
            Random rand = new Random(seed);
            int[] indexes = new int[total.Length];
            string[] corrected = new string[total.Length];
            List<int> possibleNumbers = new List<int>(Enumerable.Range(0, total.Length));
            int f = 0;
            for (int i = possibleNumbers.Count; f < fakes.Length; i--)
            {
                int x = rand.Next(i);
                corrected[possibleNumbers[x]] = fakes[f];
                f++;
                possibleNumbers[x] = possibleNumbers[i - 1];
            }
            int r = 0;
            for (int i = 0; i < corrected.Length; i++)
            {
                if (corrected[i] == null)
                {
                    corrected[i] = reals[r];
                    r++;
                }
            }
            eWatch.Stop();
            // if(new List<string>(corrected).Count != total.Length) Console.WriteLine("ERORROR ORfdsakfs kj");
            File.AppendAllText("Info.txt", "\nTime for Random Encryption: " + eWatch.ElapsedMilliseconds + "ms");
            Stopwatch dWatch = new Stopwatch();
            dWatch.Start();
            List<string> rreals = new List<string>();
            rand = new Random(seed);
            f = 0;
            possibleNumbers = new List<int>(Enumerable.Range(0, total.Length));
            for (int i = possibleNumbers.Count; f < fakes.Length; i--)
            {
                int x = rand.Next(i);
                corrected[possibleNumbers[x]] = null;
                f++;
                possibleNumbers[x] = possibleNumbers[i - 1];
            }
            for(int i = 0; i < corrected.Length; i++){
                if(corrected[i] != null){
                    rreals.Add(corrected[i]);
                }
            }
            dWatch.Stop();
            File.WriteAllLines("randomreals.txt", rreals.ToArray());
            File.AppendAllText("Info.txt", "\nTime for Random Decryption: " + dWatch.ElapsedMilliseconds + "ms");
        }
    }

    public class QuadFunction
    {
        int a = 0;
        int b = 0;
        int c = 0;

        public QuadFunction(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public int Evaluate(int x)
        {
            return (a * (x * x)) + (b * x) + c;
        }
    }
}
