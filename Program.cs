using System;

namespace SmolScript
{
    internal class SmolScriptProgram
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
                Console.WriteLine("SmolScript Interactive");

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
            var startTime = System.Environment.TickCount;

            if (interpreterInstance == null)
            {
                interpreterInstance = new Interpreter();
            }

            var scanner = new Scanner(source);
            var scanResult = scanner.ScanTokens();

            var scanTime = System.Environment.TickCount - startTime;


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

                    var parseTime = System.Environment.TickCount - startTime - scanTime;

                    if (statements != null)
                    {
                        //Console.WriteLine(new AstDebugPrinter().Print(statements));

                        interpreterInstance.Interpret(statements);

                        var executionTime = System.Environment.TickCount - startTime - scanTime - parseTime;

                        Console.WriteLine($"Done. Took {System.Environment.TickCount - startTime} ms total");
                        Console.WriteLine($"(Scan time = {scanTime}, Parse time = {parseTime}, Execution time = {executionTime})");                        
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

