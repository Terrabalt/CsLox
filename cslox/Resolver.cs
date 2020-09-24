using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Resolver : IExprVisitor<object>, IStmtVisitor<object>
    {
        Stack<Dictionary<string, bool>> scopes;
        private Interpreter interpreter;

        enum FunctionType { NONE, FUNCTION}
        FunctionType currentFunction = FunctionType.NONE;

        public Resolver(Interpreter _interpreter)
        {
            interpreter = _interpreter;
            scopes = new Stack<Dictionary<string, bool>>();
        }

        #region Statements
        public object visitStmtBlockStmt(StmtBlock stmt)
        {
            beginScope();
            resolve(stmt.statements);
            endScope();
            return null;
        }

        public object visitStmtExpressionStmt(StmtExpression stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object visitStmtFunctionStmt(StmtFunction stmt)
        {
            declare(stmt.name);
            define(stmt.name);

            resolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object visitStmtIfStmt(StmtIf stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) resolve(stmt.elseBranch);
            return null;
        }

        public object visitStmtPrintStmt(StmtPrint stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object visitStmtReturnStmt(StmtReturn stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Program.error(stmt.keyword, "Cannot return from top-level code.");
            }
            if (stmt.rValue != null) resolve(stmt.rValue);
            return null;
        }

        public object visitStmtVarStmt(StmtVar stmt)
        {
            declare(stmt.name);
            if(stmt.Initializer != null)
            {
                resolve(stmt.Initializer);
            }
            define(stmt.name);
            return null;
        }

        public object visitStmtWhileStmt(StmtWhile stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.body);
            return null;
        }
        #endregion

        #region Statement Parse Helper
        void beginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        public void resolve(List<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                resolve(statement);
            }
        }

        void resolve(Stmt statement)
        {
            try
            {
                statement.accept(this);
            }
            catch (LoxRuntimeException error)
            {
                Program.runtimeError(error);
            }
        }

        void endScope()
        {
            scopes.Pop();
        }

        void resolve(Expr expression)
        {
            expression.accept(this);
        }

        void declare(Token name)
        {
            if (scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme)) Program.error(name, "Variable with this name is already declared in this scope");
            else scope.Add(name.lexeme, false);
        }

        void define(Token name)
        {
            if (scopes.Count == 0) return;

            if (scopes.Peek().ContainsKey(name.lexeme))
                scopes.Peek()[name.lexeme] = true;
            else
                scopes.Peek().Add(name.lexeme, true);
        }
        
        void resolveFunction(StmtFunction stmt, FunctionType fType)
        {
            FunctionType encFunction = currentFunction;
            currentFunction = fType;

            beginScope();
            foreach(Token param in stmt.arguments)
            {
                declare(param);
                define(param);
            }
            resolve(stmt.body);
            endScope();

            currentFunction = encFunction;
        }
        #endregion

        #region Expressions
        public object visitExprAssignExpr(ExprAssign expr)
        {
            resolve(expr.value);
            resolveLocal(expr, expr.name);
            return null;
        }

        public object visitExprBinaryExpr(ExprBinary expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object visitExprCallExpr(ExprCall expr)
        {
            resolve(expr.callee);

            foreach (Expr arg in expr.arguments) resolve(arg);

            return null;
        }

        public object visitExprGroupingExpr(ExprGrouping expr)
        {
            resolve(expr.expression);
            return null;
        }

        public object visitExprLiteralExpr(ExprLiteral expr)
        {
            return null;
        }

        public object visitExprLogicalExpr(ExprLogical expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object visitExprTernaryExpr(ExprTernary expr)
        {
            resolve(expr.top);
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object visitExprUnaryExpr(ExprUnary expr)
        {
            resolve(expr.right);
            return null;
        }

        public object visitExprVarExpr(ExprVar expr)
        {
            bool x;
            if (scopes.Count > 0 && scopes.Peek().TryGetValue(expr.name.lexeme, out x) && !x) 
            {
                Program.error(expr.name, "Cannot read local variable in its own initializer.");
            }
            resolveLocal(expr, expr.name);
            return null;
        }
        #endregion

        #region

        void resolveLocal(Expr expr, Token name)
        {
            bool ret = false;
            Dictionary<string, bool>[] scopesArray = scopes.ToArray();
            for (int i = 0; i < scopesArray.Length; i++)
            {
                Dictionary<string, bool> scope = scopesArray[i];
                if (!ret && scope.ContainsKey(name.lexeme)) interpreter.resolve(expr, scopesArray.Length - (1 + i));
                scopes.Push(scope);
            }
        }
        #endregion
    }
}
