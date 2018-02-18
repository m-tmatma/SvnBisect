using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnBisect
{
    class Program
    {
        static void Usage()
        {
            var progname = System.AppDomain.CurrentDomain.FriendlyName;
            Console.Error.WriteLine("usage: {0} -ok rev1 -ng rev2 -- program.exe [parameters to program.exe]",
                progname);

            Console.Error.WriteLine("example:");
            Console.Error.WriteLine("{0} -ok 10 -ng 20  -- cmd.exe /c test.bat", progname);
            Console.Error.WriteLine("{0} -ok 20 -ng 10  -- cmd.exe /c test.bat", progname);
        }
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return 0;
            }
            try
            {
                var result = SvnBisect.Bisect(args);
                var tempEnum = result.Result.OrderBy((x) => x.Key);
                foreach (var v in tempEnum)
                {
                    Console.WriteLine(string.Format("result {0} : {1}", v.Key, v.Value ? "OK" : "NG"));
                }
                return 0;
            }
            catch (SvnBisect.UnknownOptionException e)
            {
                Console.Error.WriteLine("unknown option {0} found", e.Message);
                Usage();
                return 1;
            }
            catch (SvnBisect.LogFileNotFoundException)
            {
                Console.Error.WriteLine("log filename not found");
                Usage();
                return 1;
            }
            catch (SvnBisect.SeparatorNotFoundException)
            {
                Console.Error.WriteLine("separator '--' not found");
                Usage();
                return 1;
            }
            catch (SvnBisect.ArgumentNotFoundException)
            {
                Console.Error.WriteLine("arguement not found");
                Usage();
                return 1;
            }
        }
    }
}
