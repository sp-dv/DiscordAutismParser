using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAutismParser
{
    internal class KeyCity
    {
        public static KeyStruct MakeKeyPair()
        {
            var csp = new RSACryptoServiceProvider(2048);
            var keyStruct = new KeyStruct();

            keyStruct.Private = csp.ExportParameters(true);
            keyStruct.Public = csp.ExportParameters(false);

            return keyStruct;
        }

        public static string EncryptMessage(string message, KeyStruct key)
        {
            var pubKey = key.Public;
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);

            var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(message);
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);
            var cypherText = Convert.ToBase64String(bytesCypherText);

            return cypherText;
        }

        public static string DecryptMessage(string cypherText, KeyStruct key)
        {
            try
            {
                var privKey = key.Private;
                var csp = new RSACryptoServiceProvider();
                csp.ImportParameters(privKey);

                var bytesCypherText = Convert.FromBase64String(cypherText);
                var bytesPlainTextData = csp.Decrypt(bytesCypherText, false);
                var plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);

                return plainTextData;
            }
            catch
            {
                return "Tried to decrypt a message with the wrong key...";
            }
        }

        private static void foo()
        {
            //lets take a new CSP with a new 2048 bit rsa key pair
            var csp = new RSACryptoServiceProvider(2048);

            //how to get the private key
            var privKey = csp.ExportParameters(true);

            //and the public key ...
            var pubKey = csp.ExportParameters(false);

            //converting the public key into a string representation
            string pubKeyString;
            {
                //we need some buffer
                var sw = new System.IO.StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, pubKey);
                //get the string from the stream
                pubKeyString = sw.ToString();
            }

            //converting it back
            {
                //get a stream from the string
                var sr = new System.IO.StringReader(pubKeyString);
                //we need a deserializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //get the object back from the stream
                pubKey = (RSAParameters)xs.Deserialize(sr);
            }

            //conversion for the private key is no black magic either ... omitted

            //we have a public key ... let's get a new csp and load that key
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);

            //we need some data to encrypt
            var plainTextData = "foobar";

            //for encryption, always handle bytes...
            var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

            //apply pkcs#1.5 padding and encrypt our data 
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

            //we might want a string representation of our cypher text... base64 will do
            var cypherText = Convert.ToBase64String(bytesCypherText);


            /*
             * some transmission / storage / retrieval
             * 
             * and we want to decrypt our cypherText
             */

            //first, get our bytes back from the base64 string ...
            bytesCypherText = Convert.FromBase64String(cypherText);

            //we want to decrypt, therefore we need a csp and load our private key
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privKey);

            //decrypt and strip pkcs#1.5 padding
            bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

            //get our original plainText back...
            plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);

            var a = JsonConvert.SerializeObject(pubKey);
            Console.WriteLine(a);
            int x = 4;
        }
    }

    class KeyStruct
    {
        public RSAParameters Public { get; set; }
        public RSAParameters Private { get; set; }
    }
}
