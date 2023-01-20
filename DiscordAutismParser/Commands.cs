using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAutismParser
{
    abstract class Command
    {
        public static Command[] AllCommands =
        {
            new WelcomeCommand(),
            new HelpCommand(),
            new ExitCommand(),

            new MakeKeyCommand(),

            new UseEncryptionKeyCommand(),
            new UseDecryptionKeyCommand(),

            new CopyKeyCommand(),
            new ReceiveKeyCommand(),

            new EncryptMessageCommand(),
            new DecryptMessageCommand(),

        };

        public string Name { get; set; }
        public string Description { get; set; }
        public bool Callable { get; set; } = true;
        public string UsageHint { get; set; } = null;
        public bool NoArgumentParsing { get; set; } = false;

        public abstract void Run(string[] paramaters);

        public static void ParseAndRun(string input)
        {
            var trimmedInput = input.Trim();
            var words = trimmedInput.Split(' ');

            if (words.Length == 0) 
            { 
                BadInputPrompt(); 
                return; 
            }

            var cmd = words[0];

            foreach (var command in AllCommands)
            {
                if (command.Callable && command.Name == cmd)
                {
                    if (command.NoArgumentParsing)
                    {
                        command.Run(new[] { input.Substring(cmd.Length) });
                    }
                    else
                    {
                        command.Run(words.Skip(1).ToArray());
                    }
                    return;
                }
            }

            BadInputPrompt();
        }

        protected static void BadInputPrompt()
        {
            new NonsenseCommand().Run(null);
        }
    }

    class DecryptMessageCommand : Command
    {
        public DecryptMessageCommand()
        {
            Name = "recmsg";
            Description = "Dencrypts a message with the currently used decryption key.";
            UsageHint = $"{Name}";
        }

        public override void Run(string[] paramaters)
        {
            if (paramaters.Length != 0)
            {
                BadInputPrompt();
            }

            var key = Storage.GetUsedDencryptionKey();

            Console.WriteLine("Paste received cipher text then press enter:");
            var cipherText = Console.ReadLine();

            var plainText = KeyCity.DecryptMessage(cipherText, key);

            Console.WriteLine($"Super Secret Message: {plainText}");
        }
    }

    class EncryptMessageCommand : Command
    {
        public EncryptMessageCommand()
        {
            Name = "sendmsg";
            Description = "Encrypts a message with the currently used encryption key.";
            UsageHint = $"{Name} <message>";
            NoArgumentParsing = true;
        }

        public override void Run(string[] paramaters)
        {
            if (paramaters.Length != 1)
            {
                BadInputPrompt();
                return;
            }

            var key = Storage.GetUsedEncryptionKey();
            if (key == null)
            {
                Console.WriteLine("Can't encrypt without the other parties public key.");
                return;
            }

            var message = paramaters[0].Trim();

            var cipherText = KeyCity.EncryptMessage(message, key);
            WindowsClipboard.SetText(cipherText);
            Console.WriteLine("Cipher text copied to clipboard.");
        }
    }

    class ReceiveKeyCommand : Command
    {
        public ReceiveKeyCommand()
        {
            Name = "reckey";
            Description = "Creates a key from a received public key";
            UsageHint = $"{Name} <keyname> <keytext>";
        }

        public override void Run(string[] paramaters)
        {
            if (paramaters.Length != 2)
            {
                BadInputPrompt();
                return;
            }

            var keyName = paramaters[0];
            var bread = paramaters[1];

            KeyStruct key;

            try
            {
                key = JsonConvert.DeserializeObject<KeyStruct>(bread);
            }
            catch
            {
                Console.WriteLine("Malformed key.");
                return;
            }

            var success = Storage.SaveKeyPair(keyName, key);

            if (success)
            {
                Console.WriteLine("Successfully saved key.");
            }
        }
    }

    class ExitCommand : Command
    {
        public ExitCommand()
        {
            Name = "exit";
            Description = "Exists the program.";
        }

        public override void Run(string[] paramaters)
        {
            Environment.Exit(0);
        }
    }

    class CopyKeyCommand : Command
    {
        public CopyKeyCommand()
        {
            Name = "copykey";
            Description = "Copies the given key to clipboard for publication.";
            UsageHint = $"{Name} <keyname>";
        }

        public override void Run(string[] paramaters)
        {
            if (paramaters.Length != 1)
            {
                BadInputPrompt();
                return;
            }

            var keyName = paramaters[0];

            var key = Storage.LoadKeyPair(keyName);
            if (key == null)
            {
                return;
            }

            var bread = JsonConvert.SerializeObject(key.Public);
            WindowsClipboard.SetText(bread);
            Console.WriteLine("Public key copied to clipboard.");
        }
    }

    class UseEncryptionKeyCommand : Command
    {
        public UseEncryptionKeyCommand()
        {
            Name = "usepubkey";
            Description = "Set a key as currently used encryption key.";
            UsageHint = $"{Name} <keyname>";
        }

        public override void Run(string[] paramaters)
        {
            if (paramaters.Length != 1)
            {
                BadInputPrompt();
                return;
            }

            var keyName = paramaters[0];
            var success = Storage.SetUsedEncryptionKey(keyName);

            if (success)
            {
                Console.WriteLine($"Using key '{keyName}'");
            }
            else
            {
                Console.WriteLine("Failed.");
            }    
        }
    }

    class UseDecryptionKeyCommand : Command
    {
        public UseDecryptionKeyCommand()
        {
            Name = "useprivkey";
            Description = "Set a key as currently used decryption key.";
            UsageHint = $"{Name} <keyname>";
        }

        public override void Run(string[] paramaters)
        {
            if (paramaters.Length != 1)
            {
                BadInputPrompt();
                return;
            }

            var keyName = paramaters[0];
            var success = Storage.SetUsedDencryptionKey(keyName);

            if (success)
            {
                Console.WriteLine($"Using key '{keyName}'");
            }
            else
            {
                Console.WriteLine("Failed.");
            }
        }
    }

    class MakeKeyCommand : Command
    {
        public MakeKeyCommand()
        {
            Name = "makekey";
            Description = "Generates a key.";
            UsageHint = "makekey <keyname>";
        }

        public override void Run(string[] paramaters)
        {
            if (paramaters.Length != 1)
            {
                BadInputPrompt();
                return;
            }
            var keyName = paramaters[0];

            if (keyName == "used")
            {
                Console.WriteLine("Key name cannot be 'used'.");
                return;
            }

            Console.Write($"Creating key '{keyName}'... ");
            
            var key = KeyCity.MakeKeyPair();
            var saveSuccess = Storage.SaveKeyPair(keyName, key);
            
            
            if (saveSuccess) { Console.WriteLine("Done!"); }
            else { Console.WriteLine("Failed..."); }
        }
    }

    class WelcomeCommand : Command
    {
        public WelcomeCommand()
        {
            Name = "welcome";
            Description = "Shows the welcome message.";
            Callable = false;
        }

        public override void Run(string[] paramaters)
        {
            Console.WriteLine("Welcome to Discord Autism Wrangler version 0.1.");
            Console.WriteLine("Type 'help' for help.");
        }
    }

    class NonsenseCommand : Command
    {
        public NonsenseCommand()
        {
            Name = "nonsense";
            Description = "Shows the nonsense message.";
            Callable = false;
        }

        public override void Run(string[] paramaters)
        {
            Console.WriteLine("You are typing incoherent nonsense; type 'help' for help.");
        }
    }

    class HelpCommand : Command
    {
        public HelpCommand()
        {
            Name = "help";
            Description = "Shows the help message.";
        }

        public override void Run(string[] paramaters)
        {
            foreach (var command in AllCommands)
            {
                if (!command.Callable) { continue; }
                Console.WriteLine($"{command.Name} - {command.Description}{(command.UsageHint == null ? "" : $" Usage: '{command.UsageHint}'")}");
            }
        }
    }
}
