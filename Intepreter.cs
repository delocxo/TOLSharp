using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal static class Intepreter
    {
        public static void Execute(List<Stmt> ast, Scope scope)
        {
            foreach (Stmt stmt in ast)
            {
                if (stmt is VarStmt varStmt)
                {
                    Value value = Evaluator.Evaluate(varStmt.Expr, scope);
                    scope.Define(varStmt.Name, value);
                }
            }
        }

        public static void Run(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"Failed to find '{filePath}'");
                Environment.Exit(1);
            }

            try
            {
                string contents = File.ReadAllText(filePath);
                Lexer lexer = new Lexer(contents, Path.GetFullPath(filePath));

                Parser parser = new Parser(lexer.Lex());
                List<Stmt> ast = parser.Parse();

                SematicChecks sematicChecks = new SematicChecks();
                sematicChecks.Check(ast);

                Execute(ast, new Scope(null));
            }
            catch (Error error)
            {
                error.Exit();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Internal error: {e.Message}");
                Environment.Exit(1);
            }
        }
    }
}
