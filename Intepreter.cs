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

                if (stmt is IfStmt ifStmt)
                {
                    bool executeElse = true;

                    foreach (var branch in ifStmt.Branches)
                    {
                        Value value = Evaluator.Evaluate(branch.Expr, scope);
                        if (ValueFunctions.IsTruthy(value))
                        {
                            executeElse = false;

                            try
                            {
                                Execute(branch.Body, scope);
                            }
                            catch (LeaveSignal) { }
                        }
                    }

                    if (executeElse && ifStmt.ElseBody != null) 
                        try
                        {
                            Execute(ifStmt.ElseBody, scope);
                        }
                        catch (LeaveSignal) { }
                }

                if (stmt is WhileStmt whileStmt)
                {
                    bool executeElse = true;

                    while (true)
                    {
                        Value value = Evaluator.Evaluate(whileStmt.Branch.Expr, scope);

                        if (!ValueFunctions.IsTruthy(value))
                            break;

                        try
                        {
                            Execute(whileStmt.Branch.Body, scope);
                        }
                        catch (BreakSignal)
                        {
                            executeElse = false;
                            break;
                        }
                        catch (ContinueSignal)
                        {
                            continue;
                        }
                    }

                    if (executeElse && whileStmt.ElseBody != null)
                        try
                        {
                            Execute(whileStmt.ElseBody, scope);
                        }
                        catch (LeaveSignal) { }
                }

                if (stmt is BreakStmt breakStmt)
                {
                    if (breakStmt.Condition != null)
                    {
                        if (Evaluator.ExprIsTruthy(breakStmt.Condition, scope))
                            throw new BreakSignal();
                    }
                    else
                        throw new BreakSignal();
                }

                if (stmt is ContinueStmt continueStmt)
                {
                    if (continueStmt.Condition != null)
                    {
                        if (Evaluator.ExprIsTruthy(continueStmt.Condition, scope))
                            throw new ContinueSignal();
                    }
                    else
                        throw new ContinueSignal();
                }

                if (stmt is LeaveStmt leaveStmt)
                {
                    if (leaveStmt.Condition != null)
                    {
                        if (Evaluator.ExprIsTruthy(leaveStmt.Condition, scope))
                            throw new LeaveSignal();
                    }
                    else
                        throw new LeaveSignal();
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
