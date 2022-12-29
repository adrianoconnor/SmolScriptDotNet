using System;

namespace ABasic
{
    internal class ABasicProgram
    {
        static void Main(string[] args)
        {
            if (args.GetLength(0) >= 1)
            {
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine($"Error: Could not find basic programme with filename {args[0]}");
                    return;
                }
                
                string source = File.ReadAllText(args[0]);

                Run(source);
            }
            else 
            {
                Console.WriteLine("ABasic Interactive");

                var interpreterInstance = new Interpreter();

                while(true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();

                    if (!string.IsNullOrEmpty(input))
                    {                    
                        Run(input, interpreterInstance);
                    }
                }
            }
        }

        static void Run(string source, Interpreter? interpreterInstance = null)
        {
            if (interpreterInstance == null)
            {
                interpreterInstance = new Interpreter();
            }

            var scanner = new Scanner(source);
            var scanResult = scanner.ScanTokens();


            if (scanResult.errors.Any())
            {
                foreach(var error in scanResult.errors)
                {
                    Console.WriteLine($"Error on line {error.line}: {error.message}");
                }               
            }
            else
            {
                var parser = new Parser(scanResult.tokens);

                try
                {
                    var statements = parser.Parse();

                    if (statements != null)
                    {
                        //Console.WriteLine(new AstDebugPrinter().Print(expression));

                        interpreterInstance.Interpret(statements);
                    }
                }
                catch (ParseError e)
                {
                    if (e.Errors != null)
                    {
                        Console.WriteLine($"{e.Message}");

                        foreach(var error in e.Errors)
                        {
                            Console.WriteLine($"Error on line {error.LineNumber}: {error.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error on line {e.LineNumber}: {e.Message}");
                    }
                }
            }
        }
    }
}

