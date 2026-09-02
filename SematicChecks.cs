using System;
using System.Collections.Generic;
using System.Text;

namespace TOLSharp
{
    internal class SematicChecks
    {
        int _loopDepth = 0;
        int _ifDepth = 0;

        public void Check(List<Stmt> ast)
        {
            foreach (Stmt stmt in ast)
            {
                if (stmt is IfStmt ifStmt)
                    CheckIfStmt(ifStmt);

                if (stmt is WhileStmt whileStmt)
                    CheckWhileStmt(whileStmt);

                if (stmt is BreakStmt breakStmt)
                    if (_loopDepth < 1)
                        throw new Error("break cannot be used outside loops", breakStmt.Position);

                if (stmt is ContinueStmt continueStmt)
                    if (_loopDepth < 1)
                        throw new Error("continue cannot be used outside loops", continueStmt.Position);

                if (stmt is LeaveStmt leaveStmt)
                    if (_ifDepth < 1)
                        throw new Error("leave cannot be used outside ifs", leaveStmt.Position);

            }
        }

        void CheckIfStmt(IfStmt ifStmt)
        {
            foreach (var branch in ifStmt.Branches)
            {
                _ifDepth++;
                Check(branch.Body);
                _ifDepth--;
            }
            
            if (ifStmt.ElseBody != null)
            {
                _ifDepth++;
                Check(ifStmt.ElseBody);
                _ifDepth--;
            }    
        }

        void CheckWhileStmt(WhileStmt whileStmt)
        {
            _loopDepth++;
            Check(whileStmt.Branch.Body);
            _loopDepth--;

            if (whileStmt.ElseBody != null)
            {
                _ifDepth++;
                Check(whileStmt.ElseBody);
                _ifDepth--;
            }
        }
    }
}
