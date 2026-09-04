using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal class SemanticDepthInfo
    {
        public SemanticDepthInfo(int loopDepth, int ifDepth)
        {
            LoopDepth = loopDepth;
            IfDepth = ifDepth;
        }

        public int LoopDepth { get; set; }
        public int IfDepth { get; set; }
        public int FunctionDepth { get; set; }

        public SemanticDepthInfo CloneThenClear()
        {
            int prevLoopDepth = LoopDepth;
            int prevIfDepth = IfDepth;
            LoopDepth = 0;
            IfDepth = 0;
            return new SemanticDepthInfo(prevLoopDepth, prevIfDepth);
        }
        
        public void Set(SemanticDepthInfo semanticDepthInfo)
        {
            LoopDepth = semanticDepthInfo.LoopDepth;
            IfDepth = semanticDepthInfo.IfDepth;
        }
    }

    internal class SematicChecks
    {
        SemanticDepthInfo _depthInfo = new SemanticDepthInfo(0, 0);
        Stack<Dictionary<string, HashSet<int>>> _overloadStack = new Stack<Dictionary<string, HashSet<int>>>();

        public SematicChecks()
        {
            _overloadStack.Push(new());
        }

        public void Check(List<Stmt> ast)
        {
            foreach (Stmt stmt in ast)
            {
                if (stmt is IfStmt ifStmt)
                    CheckIfStmt(ifStmt);

                if (stmt is WhileStmt whileStmt)
                    CheckWhileStmt(whileStmt);

                if (stmt is BreakStmt breakStmt)
                    if (_depthInfo.LoopDepth < 1)
                        throw new Error("break cannot be used outside loops", breakStmt.Position);

                if (stmt is ContinueStmt continueStmt)
                    if (_depthInfo.LoopDepth < 1)
                        throw new Error("continue cannot be used outside loops", continueStmt.Position);

                if (stmt is LeaveStmt leaveStmt)
                    if (_depthInfo.IfDepth < 1)
                        throw new Error("leave cannot be used outside ifs", leaveStmt.Position);

                if (stmt is ActionStmt actionStmt)
                    CheckAction(actionStmt);

            }
        }

        void CheckIfStmt(IfStmt ifStmt)
        {
            foreach (var branch in ifStmt.Branches)
            {
                _depthInfo.IfDepth++;
                Check(branch.Body);
                _depthInfo.IfDepth--;
            }
            
            if (ifStmt.ElseBody != null)
            {
                _depthInfo.IfDepth++;
                Check(ifStmt.ElseBody);
                _depthInfo.IfDepth--;
            }    
        }

        void CheckWhileStmt(WhileStmt whileStmt)
        {
            _depthInfo.LoopDepth++;
            Check(whileStmt.Branch.Body);
            _depthInfo.LoopDepth--;

            if (whileStmt.ElseBody != null)
            {
                _depthInfo.IfDepth++;
                Check(whileStmt.ElseBody);
                _depthInfo.IfDepth--;
            }
        }

        void CheckAction(ActionStmt actionStmt)
        {
            actionStmt.Parameters.CheckForDuplicates(x => $"'{x}' is a duplicate parameter from action '{actionStmt.Name}'", actionStmt.Position);

            var depthInfo = _depthInfo.CloneThenClear();

            var currentScope = _overloadStack.Peek();

            if (currentScope.TryGetValue(actionStmt.Name, out var arities))
            {
                if (!arities.Add(actionStmt.Arity))
                    throw new Error($"Duplicate overload for '{actionStmt.Name}' with {actionStmt.Arity} parameter(s)", actionStmt.Position);
            }
            else
                currentScope[actionStmt.Name] = [actionStmt.Arity];

            _overloadStack.Push(new());

            Check(actionStmt.Body);

            _depthInfo.Set(depthInfo);

            _overloadStack.Pop();
        }
    }
}
