using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using RevnCompiler;

namespace Revn
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var location = Path.GetDirectoryName( Assembly.GetEntryAssembly().Location );
            var sourceCodePath = Path.Combine( location, "HelloWorld.rv" );
            var program = File.ReadAllText(sourceCodePath);


            var lexer = new Lexer(program);
            var tokens = lexer.GenerateTokens();
            foreach(var token in tokens)
            {
                Console.WriteLine($"{token.TokenType} : {token.Value}");
            }

            Parser parser = new Parser(tokens);
            IEnumerable<ASTBase> result;
            try
            {
                result = parser.Parse();
            }
            catch ( Exception e )
            {
                Console.WriteLine(e);
                Console.ReadLine();
                return;
            }
            var ilPath = Path.Combine( location, "HelloWorld.il" );
            FileStream file = File.Create(ilPath);
            using ( var writer = new StreamWriter( file ) )
            {
                writer.WriteLine(".assembly extern mscorlib { }\n.assembly test { }");
                foreach ( var astBase in result )
                {
                    var IL = astBase.GenerateIL();
                    Console.WriteLine(IL);
                    writer.WriteLine(IL);
                }
            }


            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.ReadLine();
                return;
            }

            var commandProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = Environment.GetEnvironmentVariable( "ComSpec" ),
                        Arguments = $"/c ilasm \"{ilPath}\""
                    }
                };
            commandProcess.Start();
            commandProcess.WaitForExit();

            if(commandProcess.ExitCode != 0) return;

            Console.WriteLine();
            Console.WriteLine("=======================================");
            Console.WriteLine("Execution Result");
            Console.WriteLine("=======================================");

            var executionPath = Path.Combine( location, "HelloWorld.exe" );
            var ilProcess = new Process
            {
                StartInfo =
                {
                    FileName = executionPath,
                    Arguments = "/c"
                }
            };
            ilProcess.Start();
            ilProcess.WaitForExit();

            Console.ReadLine();
        }
    }
}
