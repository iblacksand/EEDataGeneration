using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Timers;

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
                    GenerateData();
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

        static void GenerateData(){
            string[] lines = File.ReadAllLines("all-data.csv");
            DeleteFile("FakeData.csv");
            DeleteFile("RealData.csv");
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
        }

        static void GenerateEncryptedData(){
            byte[] content = File.ReadAllBytes("RealData.csv");
            DeleteFile("EncryptedRealData.csv");
            DeleteFile("key.txt");
            DeleteFile("iv.txt");
            File.WriteAllBytes("EncryptedRealData.csv",Encrypt(content, CipherMode.CBC));
        }

        static void DecryptData(){
            byte[] content = File.ReadAllBytes("EncryptedRealData.csv");
            byte[] key = File.ReadAllBytes("key.txt");
            byte[] iv = File.ReadAllBytes("iv.txt");
            DeleteFile("DecryptedRealData.txt");
            File.WriteAllBytes("DecryptedRealData.csv", Decrypt(content, key, iv, CipherMode.CBC));
        }

        public static byte[] Encrypt(byte[] input, CipherMode cipherMode)
        {
            Console.WriteLine("Encrypting Data");
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
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

         public static byte[] Decrypt(byte[] input, byte[] key, byte[] iv, CipherMode cipherMode)
        {
            Console.WriteLine("Encrypting Data");
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
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        public static void SpeedTest(){
            // StopWatch 
        }
    }
}
