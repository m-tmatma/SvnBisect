using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Usage()
        {
            var progname = System.AppDomain.CurrentDomain.FriendlyName;
            Console.Error.WriteLine("usage: {0} OKtoNG revisionThresh revisionToCheck", progname);
            Console.Error.WriteLine("usage: {0} NGtoOK revisionThresh revisionToCheck", progname);
        }
        static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Usage();
                Console.Error.WriteLine(string.Join(" ", args));
                return 1;
            }
            int revisionThresh  = int.Parse(args[1]);
            int revisionToCheck = int.Parse(args[2]);
            if (string.Compare(args[0], "OKtoNG") == 0)
            {
                if (revisionToCheck <= revisionThresh)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else if (string.Compare(args[0], "NGtoOK") == 0)
            {
                if (revisionToCheck < revisionThresh)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            Usage();
            return 1;
        }
    }
}
