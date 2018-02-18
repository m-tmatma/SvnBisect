using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SvnBisect
{
    public class SvnBisect
    {
        /// <summary>
        /// Option class
        /// </summary>
        public class Option
        {
            /// <summary>
            /// Output log filename
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// revision OK
            /// </summary>
            public int revisionOK { get; set; }

            /// <summary>
            /// revision NG
            /// </summary>
            public int revisionNG { get; set; }

            /// <summary>
            /// Argument for a program be launched
            /// </summary>
            public string[] Args { get; set; }
        }

        /// <summary>
        /// InternalOption class
        /// </summary>
        public class InternalOption
        {
            /// <summary>
            /// Output log filename
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Argument for a program be launched
            /// </summary>
            public string[] Args { get; set; }
        }

        /// <summary>
        /// InternalOption class
        /// </summary>
        public class BisectResult
        {
            /// <summary>
            /// Output log filename
            /// </summary>
            public Dictionary<int, bool> Result { get; set; }

            /// <summary>
            /// Argument for a program be launched
            /// </summary>
            public int revision { get; set; }
        }

        public class UnknownOptionException : Exception
        {
            public UnknownOptionException()
            {
            }

            public UnknownOptionException(string message)
                : base(message)
            {
            }
        }

        public class LogFileNotFoundException : Exception
        {
            public LogFileNotFoundException()
            {
            }

            public LogFileNotFoundException(string message)
                : base(message)
            {
            }
        }

        public class SeparatorNotFoundException : Exception
        {
            public SeparatorNotFoundException()
            {
            }

            public SeparatorNotFoundException(string message)
                : base(message)
            {
            }
        }

        public class ArgumentNotFoundException : Exception
        {
            public ArgumentNotFoundException()
            {
            }

            public ArgumentNotFoundException(string message)
                : base(message)
            {
            }
        }

        public class OKRevisionNotFoundException : Exception
        {
            public OKRevisionNotFoundException()
            {
            }

            public OKRevisionNotFoundException(string message)
                : base(message)
            {
            }
        }

        public class NGRevisionNotFoundException : Exception
        {
            public NGRevisionNotFoundException()
            {
            }

            public NGRevisionNotFoundException(string message)
                : base(message)
            {
            }
        }

        /// <summary>
        /// parse commandline argument
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Option ParseCommandLine(string[] args)
        {
            var mainArgs = new List<string>();
            var programArgs = new List<string>();

            bool foundSeparater = false;
            foreach (string arg in args)
            {
                if (!foundSeparater)
                {
                    if (string.Compare(arg, "--") == 0)
                    {
                        foundSeparater = true;
                        continue;
                    }
                    mainArgs.Add(arg);
                }
                else
                {
                    programArgs.Add(arg);
                }
            }
            if (!foundSeparater)
            {
                throw new SeparatorNotFoundException();
            }
            if (programArgs.Count == 0)
            {
                throw new ArgumentNotFoundException();
            }
            var option = new Option();
            option.FileName = null;
            option.revisionOK = 0;
            option.revisionNG = 0;
            option.Args = programArgs.ToArray();

            var mainArgsArray = mainArgs.ToArray();
            for (int i = 0; i < mainArgsArray.Length; i++)
            {
                var current = mainArgsArray[i];
                if (current.StartsWith("-"))
                {
                    string next = ((i + 1) < mainArgsArray.Length) ? mainArgsArray[i + 1] : null;
                    if (string.Compare(current, "-ok") == 0)
                    {
                        option.revisionOK = Int32.Parse(next);
                    }
                    else if (string.Compare(current, "-ng") == 0)
                    {
                        option.revisionNG = Int32.Parse(next);
                    }
                    else
                    {
                        throw new UnknownOptionException(current);
                    }
                }
            }
            if (option.revisionOK == 0)
            {
                throw new OKRevisionNotFoundException();
            }
            if (option.revisionNG == 0)
            {
                throw new NGRevisionNotFoundException();
            }
            return option;
        }

        public static BisectResult Bisect(string[] args)
        {
            var option = ParseCommandLine(args);
            int revisionOK = option.revisionOK;
            int revisionNG = option.revisionNG;

            var result = new BisectResult();
            result.Result = new Dictionary<int, bool>();
            result.Result[revisionOK] = true;
            result.Result[revisionNG] = false;

            if (option.revisionOK < option.revisionNG)
            {
                // degrade
                Console.WriteLine("PreCondition {0} : OK", revisionOK);
                Console.WriteLine("PreCondition {0} : NG", revisionNG);
                while (true)
                {
                    int revision = (revisionOK + revisionNG) / 2;
                    if (revision == revisionOK || revision == revisionNG)
                    {
                        break;
                    }
                    if (CheckRevision(option.Args, revision))
                    {
                        // revison OK
                        // revison (OK+NG)/2 => OK
                        // revions NG
                        revisionOK = revision;
                        result.Result[revision] = true;
                    }
                    else
                    {
                        // revison OK
                        // revison (OK+NG)/2 => NG
                        // revions NG
                        revisionNG = revision;
                        result.Result[revision] = false;
                    }
                }
                result.revision = revisionNG;
                return result;
            }
            else if (option.revisionNG < option.revisionOK)
            {
                // fixed
                Console.WriteLine("PreCondition {0} : NG", revisionNG);
                Console.WriteLine("PreCondition {0} : OK", revisionOK);
                while (true)
                {
                    int revision = (revisionOK + revisionNG) / 2;
                    if (revision == revisionOK || revision == revisionNG)
                    {
                        break;
                    }
                    if (CheckRevision(option.Args, revision))
                    {
                        // revions NG
                        // revison (OK+NG)/2 => OK
                        // revison OK
                        revisionOK = revision;
                        result.Result[revision] = true;
                    }
                    else
                    {
                        // revions NG
                        // revison (OK+NG)/2 => NG
                        // revison OK
                        revisionNG = revision;
                        result.Result[revision] = false;
                    }
                }
                result.revision = revisionOK;
                return result;
            }
            return null;
        }

        /// <summary>
        /// Check a revision is OK or not
        /// </summary>
        /// <param name="revision">a revision to check</param>
        /// <returns></returns>
        public static bool CheckRevision(string[] args, int revision)
        {
            Console.Write("Checking {0} : ", revision);

            var option = CreateOption(args, revision);
            if (Launch(option) == 0)
            {
                Console.WriteLine("OK");
                return true;
            }
            else
            {
                Console.WriteLine("NG");
                return false;
            }
        }

        public static InternalOption CreateOption(string[] args, int revision)
        {
            var list = new List<string>(args);
            list.Add(revision.ToString());
            var new_args = list.ToArray();

            var option = new InternalOption();
            option.FileName = "log-r" + revision + ".txt";
            option.Args = new_args;
            return option;
        }

        /// <summary>
        /// launch program and log to the output
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static int Launch(InternalOption option)
        {
            string argument = string.Empty;
            if (option.Args.Length < 1)
            {
                return 1;
            }
            else if (option.Args.Length > 1)
            {
                // create new array excluding the first elelent.
                var new_length = option.Args.Length - 1;
                var arguments = new string[new_length];
                Array.Copy(option.Args, 1, arguments, 0, new_length);

                // create string from the array
                argument = string.Join(" ", arguments);
             }

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = option.Args[0];
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = false;
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.Arguments = argument;

                    var encoding = Console.OutputEncoding;
                    process.StartInfo.StandardOutputEncoding = encoding;
                    process.StartInfo.StandardErrorEncoding = encoding;

                    var fileMode = FileMode.Create;

                    using (var fs = File.Open(option.FileName, fileMode, FileAccess.Write, FileShare.Read))
                    {
                        using (var streamWriter = new StreamWriter(fs))
                        {
                            process.OutputDataReceived += new DataReceivedEventHandler(delegate (object obj, DataReceivedEventArgs e)
                            {
                                if (e.Data == null)
                                {
                                    return;
                                }
                                streamWriter.WriteLine(e.Data);
                                Console.WriteLine(e.Data);
                            });
                            process.ErrorDataReceived += new DataReceivedEventHandler(delegate (object obj, DataReceivedEventArgs e)
                            {
                                if (e.Data == null)
                                {
                                    return;
                                }
                                streamWriter.WriteLine(e.Data);
                                Console.WriteLine(e.Data);
                            });
                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();
                        }
                    }
                    return process.ExitCode;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
