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
            string choices = "Actions:\n1. Generate New Fake and Real Data\n2. Generate Encrypted Data\n3. Decrypt Data";
            Console.WriteLine(choices);
            Int32.TryParse(Prompt("Choose an action.(EX: 1)"), out response);
            switch(response){
                case 1:
                    GenerateData(Prompt("What is the file for the data"),Double.Parse(Prompt("What percentage of data is fake?(.1 or .25 or ..)")));
                    break;
                case 2:
                    GenerateEncryptedData();
                    break;
                case 3:
                    DecryptData();
                    break;
                default:
                    Console.WriteLine("Invalid Choice");
                    goto LGetResponse;
            }
            Console.ReadLine();
        }

        static string Prompt(string prompt){
            Console.WriteLine(prompt);
            return Console.ReadLine();
        }

        static void DeleteFile(string path){
            if(File.Exists(path)) File.Delete(path);
        }

        static void GenerateData(string filepath, double percent){
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
            while(rngints.Count != (int)(lines.Length * percent)){
                int guess = rand.Next(lines.Length);
                if(!rngints.Contains(guess)){
                    rngints.Add(guess);
                    real.Remove(lines[guess]);
                    fake.Add(lines[guess]);
                }
            }
            File.WriteAllLines("FakeData.txt",fake.ToArray());
            File.WriteAllLines("RealData.txt", real.ToArray());
            Console.WriteLine("Done");
        }

        static void GenerateEncryptedData(){
            byte[] content = File.ReadAllBytes("RealData.txt");
            DeleteFile("EncryptedRealData.txt");
            DeleteFile("key.txt");
            DeleteFile("iv.txt");
            File.WriteAllBytes("EncryptedRealData.txt",Encrypt(content, CipherMode.CBC));
        }

        static void DecryptData(){
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
                DeleteFile("Info.txt");
                File.WriteAllText("Info.txt","Time for Encryption: " + watch.ElapsedMilliseconds + "ms");
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
                        val= msEncrypt.ToArray();
                    }
                }
            }
            watch.Stop();
            File.AppendAllText("Info.txt","\nTime for Decryption: " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine("Finished Decrypting");
            return val;
        }

        public static void SpeedTest(){
            // StopWatch 
        }
    }
}
