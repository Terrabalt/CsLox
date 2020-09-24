using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Program
    {
        static Interpreter interpreter;
        static bool hadError;
        static bool hadRuntimeError;
        static string codeName = "Lox";
        static bool isREPL;
        static int Main(string[] args)
        {
            hadError = false;
            hadRuntimeError = false;
            interpreter = new Interpreter();
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cs" + codeName + " [script]");
                hadError = true;
            } else if (args.Length == 1)
            {
                isREPL = false;
                runFile(args[0]);
            } else
            {
                isREPL = true;
                Console.WriteLine("C# " + codeName + " Interpreter.");
                runPrompt();
            }
            if (hadError)
            {
                return 65;
            }
            if (hadRuntimeError)
            {
                return 70;
            }
            return 0;
        }

        static void runFile(string path){
            string fileText = System.IO.File.ReadAllText(path);
            run(fileText);
            if (hadError) return;
            if (hadRuntimeError) return;
        }

        static void runPrompt()
        {
            bool exit = false;
            string r;
            while (!exit)
            {
                Console.Write("> ");
                r = Console.ReadLine();
                if (StringComparer.OrdinalIgnoreCase.Compare(r, "exit") == 0) exit = true;
                else if (StringComparer.OrdinalIgnoreCase.Compare(r, "quit") == 0) exit = true;
                else run(r);
                hadError = false;
                hadRuntimeError = false;
            }
        }
        public static void run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            /*foreach (Token token in tokens)
            {
                Console.WriteLine(token.toString());
            } */
            
            Parser parser = new cslox.Parser(tokens);
            List<Stmt> statements = parser.parse();

            if (hadError) return;

            //Console.WriteLine(new ExprPrinter().print(expression));
            //Console.WriteLine(new PolishNotationExprPrinter().print(expression));

            Resolver resolver = new Resolver(interpreter);
            resolver.resolve(statements);

            if (hadError) return;

            interpreter.Interpret(statements);
            
        }

        public static void printInREPL(string message)
        {
            if (isREPL) Console.WriteLine(message);
        }

        public static void error(int line, string message)
        {
            report(line, "", message);
        }

        public static void error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
                report(token.line, " at end", message);
            else
                report(token.line, " at '" + token.lexeme + '\'', message);
        }

        internal static void runtimeError(LoxRuntimeException error)
        {
            Console.WriteLine(error.Message + "\n[line " + error.token.line + ']');
            hadRuntimeError = true;
        }

        static void report(int line, string where, string message)
        {
            hadError = true;
            throw new ApplicationException("[line " + line + "] Error" + where + ": " + message);
        }
    }
}
