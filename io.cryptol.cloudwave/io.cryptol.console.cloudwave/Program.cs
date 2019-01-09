using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using io.cryptol.cloudwave;
using System.IO;

namespace io.cryptol.console.cloudwave
{
    class Program
    {
        static void Main(string[] args)
        {
            Cryptol crp = new Cryptol();
            EncSettings settings = new EncSettings();

            settings.Type = EncSettings.encType.Encrypt;
            settings.inputFile = @"\Desktop\file.odt";
            settings.outputFile = @"\Desktop\file.odt.cw";
            settings.password = "12345";

            Console.WriteLine(crp.CryptolLoad(settings));
            Console.Read();
        }
    }
}
