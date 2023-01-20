using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAutismParser
{
    internal class Storage
    {
        private const string DecryptionKeyName = "used_d";
        private const string EncryptionKeyName = "used_e";

        public static bool SetUsedEncryptionKey(string name)
        {
            var key = LoadKeyPair(name);
            if (key == null)
            {
                return false;
            }

            SaveKeyPair(EncryptionKeyName, key, true);
            return true;
        }

        public static KeyStruct? GetUsedEncryptionKey()
        {
            return LoadKeyPair(EncryptionKeyName);
        }

        public static bool SetUsedDencryptionKey(string name)
        {
            var key = LoadKeyPair(name);
            if (key == null)
            {
                return false;
            }

            SaveKeyPair(DecryptionKeyName, key, true);
            return true;
        }

        public static KeyStruct? GetUsedDencryptionKey()
        {
            return LoadKeyPair(DecryptionKeyName);
        }

        public static bool SaveKeyPair(string name, KeyStruct key, bool overwrite = false)
        {
            var fileName = GetFileName(name);

            if (!overwrite && File.Exists(fileName))
            {
                Console.WriteLine("Key already exists.");
                return false;
            }

            var bread = JsonConvert.SerializeObject(key);
            File.WriteAllText(fileName, bread);

            return true;
        }

        public static KeyStruct? LoadKeyPair(string name)
        {
            var fileName = GetFileName(name);

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"Key '{name}' doesn't exist.");
                return null;
            }

            var bread = File.ReadAllText(fileName);
            var key = JsonConvert.DeserializeObject<KeyStruct>(bread);
            return key;
        }

        public static KeyStruct[] GetAllKeys()
        {
            List<KeyStruct> keys = new List<KeyStruct>();

            var files = Directory.GetFiles("./");

            foreach (var file in files)
            {
                if (file.EndsWith(".key"))
                {
                    var bread = File.ReadAllText(file);

                    try
                    {
                        var key = JsonConvert.DeserializeObject<KeyStruct>(bread);
                        keys.Add(key);
                    }
                    catch
                    {
                        Console.WriteLine("Found malformed key.");
                    }
                }
            }

            return keys.ToArray();
        }

        private static string GetFileName(string keyName)
        {
            return $"./{keyName}.key";
        }
    }
}
