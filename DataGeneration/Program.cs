using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration {
    class Program {
        static void Main (string[] args) {
            int response = 0;
            LGetResponse:
                string choices = "Actions:\n1. Generate New Fake and Real Data\n2. Generate Encrypted Data\n3. Decrypt Data\n4. Linear Test\n5. Quadratic Test\n6. Test Random Number Generation with same Seed\n 7. Random Index Speed\n8. Multiple Encrypted Run";
            Console.WriteLine (choices);
            Int32.TryParse (Prompt ("Choose an action.(EX: 1)"), out response);
            switch (response) {
                case -1:
                    break;
                case 1:
                    GenerateData (Prompt ("What is the file for the data"), Double.Parse (Prompt ("What percentage of data is fake?(.1 or .25 or ..)")));
                    break;
                case 2:
                    GenerateEncryptedData ();
                    break;
                case 3:
                    DecryptData ();
                    break;
                case 4:
                    SpeedTest ();
                    break;
                case 5:
                    Quadratic ();
                    Console.WriteLine ("Done");
                    break;
                case 6:
                    int seed = Int32.Parse (Prompt ("Pick a seed:"));
                    Random rr = new Random (seed);
                    Console.WriteLine (rr.Next (10000) + "\n" + rr.Next (10000) + "\n" + rr.Next (10000));
                    rr = new Random (seed);
                    Console.WriteLine ("-----------");
                    Console.WriteLine (rr.Next (10000) + "\n" + rr.Next (10000) + "\n" + rr.Next (10000));
                    break;
                case 7:
                    RandomIndexes ();
                    break;
                case 8:
                    multirunEncryption (Int32.Parse (Prompt ("How many runs to complete?")));
                    break;
                case 9:
                    multirunRandom (Int32.Parse (Prompt ("How many runs to complete?")));
                    break;
                case 10:
                    multirunDecryption (Int32.Parse (Prompt ("How many runs to complete?")));
                    break;
                case 11:
                    multirunLinear(Int32.Parse (Prompt ("How many runs to complete?")));
                    break;
                    case 12:
                    multirunQuadratic(Int32.Parse (Prompt ("How many runs to complete?")));
                    break;
                default:
                    Console.WriteLine ("Invalid Choice");
                    goto LGetResponse;
            }
            Console.WriteLine ("FINISHED");
            Console.ReadLine ();
        }

        static string Prompt (string prompt) {
            Console.WriteLine (prompt);
            return Console.ReadLine ();
        }

        static void DeleteFile (string path) {
            if (File.Exists (path)) File.Delete (path);
        }

        static void GenerateData (string filepath, double percent) {
            string[] lines = File.ReadAllLines (filepath);
            DeleteFile ("FakeData.txt");
            DeleteFile ("RealData.txt");
            Console.WriteLine ("Creating Fake Data");
            string[] fakedata = new string[(int) (lines.Length * percent)];
            List<string> fake = new List<String> ();
            List<string> real = new List<String> (lines);
            List<int> rngints = new List<int> ();
            Random rand = new Random ();
            Console.WriteLine ("Generating index");
            while (rngints.Count != (int) (lines.Length * percent)) {
                int guess = rand.Next (lines.Length);
                if (!rngints.Contains (guess)) {
                    rngints.Add (guess);
                    real.Remove (lines[guess]);
                    fake.Add (lines[guess]);
                }
            }
            File.WriteAllLines ("FakeData.txt", fake.ToArray ());
            File.WriteAllLines ("RealData.txt", real.ToArray ());
            Console.WriteLine ("Done");
        }

        static void GenerateEncryptedData () {
            byte[] content = File.ReadAllBytes ("RealData.txt");
            DeleteFile ("EncryptedRealData.txt");
            DeleteFile ("key.txt");
            DeleteFile ("iv.txt");
            File.WriteAllBytes ("EncryptedRealData.txt", Encrypt (content, CipherMode.CBC));
        }

        static void DecryptData () {
            byte[] content = File.ReadAllBytes ("EncryptedRealData.txt");
            byte[] key = File.ReadAllBytes ("key.txt");
            byte[] iv = File.ReadAllBytes ("iv.txt");
            DeleteFile ("DecryptedRealData.txt");
            File.WriteAllBytes ("DecryptedRealData.txt", Decrypt (content, key, iv, CipherMode.CBC));
        }

        public static byte[] Encrypt (byte[] input, CipherMode cipherMode) {
            Console.WriteLine ("Encrypting Data");
            Stopwatch watch = new Stopwatch ();
            byte[] val;
            watch.Start ();
            using (RijndaelManaged myRijndael = new RijndaelManaged { Mode = cipherMode }) {

                myRijndael.GenerateKey ();
                myRijndael.GenerateIV ();

                File.WriteAllBytes ("key.txt", myRijndael.Key);
                File.WriteAllBytes ("iv.txt", myRijndael.IV);
                ICryptoTransform encryptor = myRijndael.CreateEncryptor (myRijndael.Key, myRijndael.IV);

                using (MemoryStream msEncrypt = new MemoryStream ()) {
                    using (CryptoStream csEncrypt = new CryptoStream (msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        csEncrypt.Write (input, 0, input.Length);
                        csEncrypt.FlushFinalBlock ();
                        val = msEncrypt.ToArray ();
                    }
                }
                watch.Stop ();
                // DeleteFile("Info.txt");
                File.AppendAllText ("Info.txt", "\nTime for Encryption: " + watch.ElapsedMilliseconds + "ms");
                Console.WriteLine ("Finished Encrypting");
                return val;
            }
        }

        public static byte[] Decrypt (byte[] input, byte[] key, byte[] iv, CipherMode cipherMode) {
            Console.WriteLine ("Decrypting Data");
            Stopwatch watch = new Stopwatch ();
            byte[] val;
            watch.Start ();
            using (RijndaelManaged myRijndael = new RijndaelManaged { Mode = cipherMode }) {

                myRijndael.IV = iv;
                myRijndael.Key = key;
                ICryptoTransform encryptor = myRijndael.CreateDecryptor (myRijndael.Key, myRijndael.IV);

                using (MemoryStream msEncrypt = new MemoryStream ()) {
                    using (CryptoStream csEncrypt = new CryptoStream (msEncrypt, encryptor, CryptoStreamMode.Write)) {
                        csEncrypt.Write (input, 0, input.Length);
                        csEncrypt.FlushFinalBlock ();
                        val = msEncrypt.ToArray ();
                    }
                }
            }
            watch.Stop ();
            File.AppendAllText ("Info.txt", "\nTime for Decryption: " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine ("Finished Decrypting");
            return val;
        }

        // string[] dict = everything.split('\n')

        public static void SpeedTest () {
            int variation = 10;
            string[] total = File.ReadAllLines ("AllData.txt");
            string[] fakes = File.ReadAllLines ("FakeData.txt");
            string[] reals = File.ReadAllLines ("RealData.txt");
            string[] corrected = new string[total.Length];
            Stopwatch watch = new Stopwatch ();
            Console.WriteLine ("Building total array");
            watch.Start ();
            int f = 0;
            int r = 0;
            for (int i = 0; i < total.Length / variation; i++) {
                corrected[variation * i] = fakes[f];
                f++;
                for (int j = 1; j < variation; j++) {
                    corrected[variation * i + j] = reals[r];
                    r++;
                }
            }
            if (corrected.Length != total.Length) Console.WriteLine ("ERROR NOT RIGHT BRO");
            watch.Stop ();
            File.AppendAllText ("Info.txt", "\nTime for Linear Encryption: " + watch.ElapsedMilliseconds + "ms");
            Stopwatch swatch = new Stopwatch ();
            List<string> realdata = new List<string> ();
            string[] real = new string[reals.Length];
            swatch.Start ();
            int c = 0;
            for (int i = 0; i < corrected.Length; i++) {
                if (i % variation != 0) {
                    real[c] = corrected[i];
                    c++;
                }
            }
            swatch.Stop ();
            File.AppendAllText ("Info.txt", "\nTime for Linear Decryption: " + swatch.ElapsedMilliseconds + "ms");
            File.WriteAllLines ("LinearReals.txt", real);
            Console.WriteLine ("Finished");
        }

        public static int SafeIndex (int[] indexes, int index, int max) {
            if (indexes.Contains (index)) {
                return SafeIndex (indexes, (index + 1) % max, max);
            }
            return index;
        }

        public static void Quadratic () {
            string[] total = File.ReadAllLines ("AllData.txt");
            string[] fakes = File.ReadAllLines ("FakeData.txt");
            string[] reals = File.ReadAllLines ("RealData.txt");
            string[] corrected = new string[total.Length];
            int[] indexes = new int[fakes.Length];
            Stopwatch eWatch = new Stopwatch ();
            int seed = Int32.Parse (Prompt ("What seed do you want? (32 bit int only)"));
            eWatch.Start ();
            // Console.Write(indexes[1]);
            Random rand = new Random (seed);
            QuadFunction fun = new QuadFunction (rand.Next (1000), rand.Next (1000), rand.Next (1000));
            // Conso/le.WriteLine("Fakes: " + fakes.Length + "\nIndexes:" + indexes.Length);
            for (int i = 0; i < fakes.Length; i++) {
                int index = SafeIndex (indexes, Math.Abs (fun.Evaluate (i * seed) % total.Length), total.Length);
                // Console.WriteLine(index);
                corrected[index] = fakes[i];
                // Console.WriteLine(i);
                indexes[i] = index;
            }
            int r = 0;
            for (int i = 0; i < total.Length; i++) {
                if (r >= reals.Length) {
                    Console.Write ("ERRORORRORORORORO\n");
                    break;
                }
                if (!indexes.Contains (i)) {
                    corrected[i] = reals[r];
                    r++;
                }
            }
            eWatch.Stop ();
            File.AppendAllText ("Info.txt", "\nTime for Quad Encryption: " + eWatch.ElapsedMilliseconds + "ms");
            bool dupsFound = false;

            for (int i = 0; i < total.Length; i++) {
                if (corrected[i] == null) {
                    dupsFound = true;
                    break;
                }
                if (corrected[i].Trim ().Equals ("")) {
                    dupsFound = true;
                    break;
                }
            }
            if (dupsFound) Console.WriteLine ("FOUND DUPLICATES");
            Stopwatch dWatch = new Stopwatch ();
            dWatch.Start ();
            string[] eReals = new string[reals.Length];
            for (int i = 0; i < fakes.Length; i++) {
                int index = SafeIndex (indexes, Math.Abs (fun.Evaluate (i * seed) % total.Length), total.Length);
                corrected[index] = "";
            }
            int rr = 0;

            for (int i = 0; i < total.Length; i++) {
                if (rr >= eReals.Length) break;
                if (corrected[i] != null && !corrected[i].Trim ().Equals ("")) {
                    eReals[rr] = corrected[i];
                    rr++;
                }
            }
            dWatch.Stop ();
            File.AppendAllText ("Info.txt", "\nTime for Quad Decryption: " + dWatch.ElapsedMilliseconds + "ms");
            File.WriteAllLines ("QuadReals.txt", eReals);
            Console.WriteLine ("Finished");
        }

        public static void Shifty () {

        }

        public static void RandomIndexes () {
            int seed = Int32.Parse (Prompt ("Pick a seed:"));

            string[] total = File.ReadAllLines ("AllData.txt");
            string[] fakes = File.ReadAllLines ("FakeData.txt");
            string[] reals = File.ReadAllLines ("RealData.txt");
            Stopwatch eWatch = new Stopwatch ();
            eWatch.Start ();
            Random rand = new Random (seed);
            int[] indexes = new int[total.Length];
            string[] corrected = new string[total.Length];
            List<int> possibleNumbers = new List<int> (Enumerable.Range (0, total.Length));
            int f = 0;
            for (int i = possibleNumbers.Count; f < fakes.Length; i--) {
                int x = rand.Next (i);
                corrected[possibleNumbers[x]] = fakes[f];
                f++;
                possibleNumbers[x] = possibleNumbers[i - 1];
            }
            int r = 0;
            for (int i = 0; i < corrected.Length; i++) {
                if (corrected[i] == null) {
                    corrected[i] = reals[r];
                    r++;
                }
            }
            eWatch.Stop ();
            // if(new List<string>(corrected).Count != total.Length) Console.WriteLine("ERORROR ORfdsakfs kj");
            File.AppendAllText ("Info.txt", "\nTime for Random Encryption: " + eWatch.ElapsedMilliseconds + "ms");
            Stopwatch dWatch = new Stopwatch ();
            dWatch.Start ();
            List<string> rreals = new List<string> ();
            rand = new Random (seed);
            f = 0;
            possibleNumbers = new List<int> (Enumerable.Range (0, total.Length));
            for (int i = possibleNumbers.Count; f < fakes.Length; i--) {
                int x = rand.Next (i);
                corrected[possibleNumbers[x]] = null;
                f++;
                possibleNumbers[x] = possibleNumbers[i - 1];
            }
            for (int i = 0; i < corrected.Length; i++) {
                if (corrected[i] != null) {
                    rreals.Add (corrected[i]);
                }
            }
            dWatch.Stop ();
            File.WriteAllLines ("randomreals.txt", rreals.ToArray ());
            File.AppendAllText ("Info.txt", "\nTime for Random Decryption: " + dWatch.ElapsedMilliseconds + "ms");
        }

        static void multirunQuadratic(int runs){
            List<string> edata = new List<string>();
            List<string> ddata = new List<string>();
            for(int z = 0; z < runs; z++){
            string[] total = File.ReadAllLines ("AllData.txt");
            string[] fakes = File.ReadAllLines ("FakeData.txt");
            string[] reals = File.ReadAllLines ("RealData.txt");
            string[] corrected = new string[total.Length];
            int[] indexes = new int[fakes.Length];
            Stopwatch eWatch = new Stopwatch ();
            int seed = Int32.Parse (Prompt ("What seed do you want? (32 bit int only)"));
            eWatch.Start ();
            // Console.Write(indexes[1]);
            Random rand = new Random (seed);
            QuadFunction fun = new QuadFunction (rand.Next (1000), rand.Next (1000), rand.Next (1000));
            // Conso/le.WriteLine("Fakes: " + fakes.Length + "\nIndexes:" + indexes.Length);
            for (int i = 0; i < fakes.Length; i++) {
                int index = SafeIndex (indexes, Math.Abs (fun.Evaluate (i * seed) % total.Length), total.Length);
                // Console.WriteLine(index);
                corrected[index] = fakes[i];
                // Console.WriteLine(i);
                indexes[i] = index;
            }
            int r = 0;
            for (int i = 0; i < total.Length; i++) {
                if (r >= reals.Length) {
                    Console.Write ("ERRORORRORORORORO\n");
                    break;
                }
                if (!indexes.Contains (i)) {
                    corrected[i] = reals[r];
                    r++;
                }
            }
            eWatch.Stop ();
            File.AppendAllText ("Info.txt", "\nTime for Quad Encryption: " + eWatch.ElapsedMilliseconds + "ms");
            bool dupsFound = false;

            for (int i = 0; i < total.Length; i++) {
                if (corrected[i] == null) {
                    dupsFound = true;
                    break;
                }
                if (corrected[i].Trim ().Equals ("")) {
                    dupsFound = true;
                    break;
                }
            }
            if (dupsFound) Console.WriteLine ("FOUND DUPLICATES");
            Stopwatch dWatch = new Stopwatch ();
            dWatch.Start ();
            string[] eReals = new string[reals.Length];
            for (int i = 0; i < fakes.Length; i++) {
                int index = SafeIndex (indexes, Math.Abs (fun.Evaluate (i * seed) % total.Length), total.Length);
                corrected[index] = "";
            }
            int rr = 0;

            for (int i = 0; i < total.Length; i++) {
                if (rr >= eReals.Length) break;
                if (corrected[i] != null && !corrected[i].Trim ().Equals ("")) {
                    eReals[rr] = corrected[i];
                    rr++;
                }
            }
            dWatch.Stop ();
            ddata.Add ((z + 1) + "," + dWatch.ElapsedMilliseconds);
                edata.Add ((z + 1) + "," + eWatch.ElapsedMilliseconds);
            }
            File.WriteAllLines ("QuadEncryptionRun.csv", edata.ToArray ());
            AverageTaker ("QuadEncryptionRun.csv");
            File.WriteAllLines ("QuadDecryptionRun.csv", ddata.ToArray ());
            AverageTaker ("QuadDecryptionRun.csv");
            
        }

        static void multirunEncryption (int runs) {
            List<string> data = new List<string> ();
            byte[] input = File.ReadAllBytes ("RealData.txt");
            byte[] key = File.ReadAllBytes ("key.txt");
            byte[] iv = File.ReadAllBytes ("iv.txt");
            CipherMode cipherMode = CipherMode.CBC;
            for (int i = 0; i < runs; i++) {
                Stopwatch watch = new Stopwatch ();
                byte[] val;
                watch.Start ();
                using (RijndaelManaged myRijndael = new RijndaelManaged { Mode = cipherMode }) {
                    myRijndael.Key = key;
                    myRijndael.IV = iv;
                    ICryptoTransform encryptor = myRijndael.CreateEncryptor (myRijndael.Key, myRijndael.IV);

                    using (MemoryStream msEncrypt = new MemoryStream ()) {
                        using (CryptoStream csEncrypt = new CryptoStream (msEncrypt, encryptor, CryptoStreamMode.Write)) {
                            csEncrypt.Write (input, 0, input.Length);
                            csEncrypt.FlushFinalBlock ();
                            val = msEncrypt.ToArray ();
                        }
                    }
                    watch.Stop ();
                    data.Add ((i + 1) + "," + watch.ElapsedMilliseconds);
                }
            }
            File.WriteAllLines ("EncryptedRunData.csv", data.ToArray ());
            AverageTaker ("EncryptedRunData.csv");

        }

        static void multirunDecryption (int runs) {
            List<string> data = new List<string> ();
            byte[] content = File.ReadAllBytes ("EncryptedRealData.txt");
            byte[] input = content;
            byte[] key = File.ReadAllBytes ("key.txt");
            byte[] iv = File.ReadAllBytes ("iv.txt");
            CipherMode cipherMode = CipherMode.CBC;
            for (int i = 0; i < runs; i++) {
                    Stopwatch watch = new Stopwatch ();
                    byte[] val;
                    watch.Start ();
                    using (RijndaelManaged myRijndael = new RijndaelManaged { Mode = cipherMode }) {

                        myRijndael.IV = iv;
                        myRijndael.Key = key;
                        ICryptoTransform encryptor = myRijndael.CreateDecryptor (myRijndael.Key, myRijndael.IV);

                        using (MemoryStream msEncrypt = new MemoryStream ()) {
                            using (CryptoStream csEncrypt = new CryptoStream (msEncrypt, encryptor, CryptoStreamMode.Write)) {
                                csEncrypt.Write (input, 0, input.Length);
                                csEncrypt.FlushFinalBlock ();
                                val = msEncrypt.ToArray ();
                            }
                        }
                    }
                    watch.Stop ();
                    data.Add ((i + 1) + "," + watch.ElapsedMilliseconds);
                }
            File.WriteAllLines ("DecryptedRunData.csv", data.ToArray ());
            AverageTaker("DecryptedRunData.csv");
        }

    static void multirunLinear(int runs){
            List<string> edata = new List<string>();
            List<string> ddata = new List<string>();
            int variation = 10;
            
            for(int i = 0; i < runs; i++){
                string[] total = File.ReadAllLines ("AllData.txt");
            string[] fakes = File.ReadAllLines ("FakeData.txt");
            string[] reals = File.ReadAllLines ("RealData.txt");
            string[] corrected = new string[total.Length];
            Stopwatch watch = new Stopwatch ();
            // Console.WriteLine ("Building total array");
            watch.Start ();
            int f = 0;
            int r = 0;
            for (int j = 0; j < total.Length / variation; j++) {
                corrected[variation * j] = fakes[f];
                f++;
                for (int k = 1; k < variation; k++) {
                    corrected[variation * j + k] = reals[r];
                    r++;
                }
            }
            if (corrected.Length != total.Length) Console.WriteLine ("ERROR NOT RIGHT BRO");
            watch.Stop ();
            edata.Add((i+1) + "," + watch.ElapsedMilliseconds);
            Stopwatch swatch = new Stopwatch ();
            List<string> realdata = new List<string> ();
            string[] real = new string[reals.Length];
            swatch.Start ();
            int c = 0;
            for (int j = 0; j < corrected.Length; j++) {
                if (j % variation != 0) {
                    real[c] = corrected[j];
                    c++;
                }
            }
            swatch.Stop ();
            ddata.Add((i+1) + "," + swatch.ElapsedMilliseconds);
            }
            File.WriteAllLines("LinearEncryptionRuns.txt", edata.ToArray());
            AverageTaker("LinearEncryptionRuns.txt");
            File.WriteAllLines("LinearDecryptionRuns.txt", ddata.ToArray());
            AverageTaker("LinearDecryptionRuns.txt");
    }

        

        static void multirunRandom (int runs) {
            List<string> edata = new List<string> ();
            List<string> ddata = new List<string> ();
            string[] total = File.ReadAllLines ("AllData.txt");
            string[] fakes = File.ReadAllLines ("FakeData.txt");
            string[] reals = File.ReadAllLines ("RealData.txt");
            int seed = Int32.Parse (Prompt ("Pick a seed:"));
            for (int i = 0; i < runs; i++) {
                Stopwatch eWatch = new Stopwatch ();
                eWatch.Start ();
                Random rand = new Random (seed);
                int[] indexes = new int[total.Length];
                string[] corrected = new string[total.Length];
                List<int> possibleNumbers = new List<int> (Enumerable.Range (0, total.Length));
                int f = 0;
                for (int j = possibleNumbers.Count; f < fakes.Length; j--) {
                    int x = rand.Next (j);
                    corrected[possibleNumbers[x]] = fakes[f];
                    f++;
                    possibleNumbers[x] = possibleNumbers[j - 1];
                }
                int r = 0;
                for (int j = 0; j < corrected.Length; j++) {
                    if (corrected[j] == null) {
                        corrected[j] = reals[r];
                        r++;
                    }
                }
                eWatch.Stop ();
                Stopwatch dWatch = new Stopwatch ();
                dWatch.Start ();
                List<string> rreals = new List<string> ();
                rand = new Random (seed);
                f = 0;
                possibleNumbers = new List<int> (Enumerable.Range (0, total.Length));
                for (int j = possibleNumbers.Count; f < fakes.Length; j--) {
                    int x = rand.Next (j);
                    corrected[possibleNumbers[x]] = null;
                    f++;
                    possibleNumbers[x] = possibleNumbers[j - 1];
                }
                for (int j = 0; j < corrected.Length; j++) {
                    if (corrected[j] != null) {
                        rreals.Add (corrected[j]);
                    }
                }
                dWatch.Stop ();
                ddata.Add ((i + 1) + "," + dWatch.ElapsedMilliseconds);
                edata.Add ((i + 1) + "," + eWatch.ElapsedMilliseconds);
            }
            File.WriteAllLines ("RandomEncryptionRun.csv", edata.ToArray ());
            AverageTaker ("RandomEncryptionRun.csv");
            File.WriteAllLines ("RandomDecryptionRun.csv", ddata.ToArray ());
            AverageTaker ("RandomDecryptionRun.csv");
        }

        static void AverageTaker (string path) {
            string[] file = File.ReadAllLines (path);
            double total = 0;
            for (int i = 0; i < file.Length; i++) {
                total += Double.Parse (file[i].Split (',') [1]);
            }
            double avg = total / file.Length;
            File.AppendAllText (path, "\nAVERGAGE: " + avg);
        }

        public class QuadFunction {
            int a = 0;
            int b = 0;
            int c = 0;

            public QuadFunction (int a, int b, int c) {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public int Evaluate (int x) {
                return (a * (x * x)) + (b * x) + c;
            }
        }
    }
}