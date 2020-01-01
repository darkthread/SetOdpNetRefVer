using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SetOdpNetRefVer
{
    class Program
    {
        static byte[] ConvVerStrToBytes(string verString)
        {
            var m = Regex.Match(verString, @"(?<n0>\d+)\.(?<n1>\d+)\.(?<n2>\d+)\.(?<n3>\d+)");
            if (!m.Success) throw new ApplicationException($"Invalid version format - {verString}");
            byte[] buff = new byte[8];
            for (var i = 0; i < 4; i++)
                BitConverter.GetBytes(short.Parse(m.Groups["n" + i].Value)).CopyTo(buff, i * 2);
            return buff;
        }

        static void Main(string[] args)
        {
            if (args.Length != 1 && args.Length != 3)
            {
                Console.WriteLine("Syntax: SetAsmRefDllVer.exe AssemblyName.dll");
                Console.WriteLine("Syntax: SetAsmRefDllVer.exe AssemblyName.dll 2.122.1.0 2.112.2.0");
                return;
            }
            var dllPath = args[0];
            if (!File.Exists(dllPath))
            {
                Console.WriteLine("File not found - " + dllPath);
                return;
            }

            if (args.Length == 3)
                ChangeRefVersion(dllPath, args[1], args[2]);
            else
                ShowRefVersions(dllPath);
        }

        private static void ShowRefVersions(string dllPath)
        {
            var asm = Assembly.LoadFrom(dllPath);
            asm.GetReferencedAssemblies().ToList().ForEach(o =>
            {
                Console.WriteLine($"{o.Name}, {o.Version}");
            });
        }

        private static void ChangeRefVersion(string dllPath, string oldVerStr, string newVerStr)
        {
            try
            {
                var oldVerBytes = ConvVerStrToBytes(oldVerStr);
                var newVerBytes = ConvVerStrToBytes(newVerStr);
                byte[] buff = File.ReadAllBytes(dllPath);
                var idx = 0;
                var foundCount = 0;
                while (idx < buff.Length - 8)
                {
                    var match = true;
                    for (var offset = 0; offset < 8; offset++)
                    {
                        if (buff[idx + offset] != oldVerBytes[offset])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        for (var offset = 0; offset < 8; offset++)
                            buff[idx + offset] = newVerBytes[offset];
                        foundCount++;
                    }
                    idx++;
                }
                if (foundCount == 0)
                    Console.WriteLine("Not found");
                else if (foundCount > 1)
                    Console.WriteLine("More than one matched result");
                else if (foundCount == 1)
                {
                    var newFileName = dllPath + ".patched";
                    File.WriteAllBytes(newFileName, buff);
                    Console.WriteLine("Done. Saved as " + newFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*** ERROR ***");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
