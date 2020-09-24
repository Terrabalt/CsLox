using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Interpreter : IExprVisitor<object>, IStmtVisitor<object>
    {
        public Enviroment Globals
        {
            get { return globals; }
        }
        Enviroment globals = new Enviroment();
        Enviroment enviroment;

        Dictionary<Expr, int> locals;

        public Interpreter()
        {
            enviroment = globals;
            globals.define("clock", new NativeClockFunc());
            locals = new Dictionary<Expr, int>();
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            catch (LoxRuntimeException error)
            {
                Program.runtimeError(error);
            }
        }

        void execute (Stmt s)
        {
            s.accept(this);
        }
        #region STATEMENTS
        public object visitStmtBlockStmt(StmtBlock stmt)
        {
            executeBlock(stmt.statements, new Enviroment(enviroment));
            return null;
        }

        public object visitStmtFunctionStmt(StmtFunction stmt)
        {
            LoxFunction func = new LoxFunction(stmt, enviroment);
            enviroment.define(stmt.name.lexeme, func);
            return null;
        }

        public object visitStmtIfStmt(StmtIf stmt)
        {
            if(isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.thenBranch);
            } else if (stmt.elseBranch != null)
            {
                execute(stmt.elseBranch);
            }

            return null;
        }

        public object visitStmtExpressionStmt(StmtExpression stmt)
        {
            object ret = evaluate(stmt.expression);
            if (ret != null) Program.printInREPL(toString(ret));
            
            return null;
        }

        public object visitStmtPrintStmt(StmtPrint stmt)
        {
            Object v = evaluate(stmt.expression);
            Console.WriteLine(toString(v));
            return null;
        }

        public object visitStmtReturnStmt(StmtReturn stmt)
        {
            object value = null;
            if (stmt.rValue != null) value = evaluate(stmt.rValue);

            throw new ReturnValueNonException(value);
        }

        public object visitStmtVarStmt(StmtVar stmt)
        {
            object value = null;
            if (stmt.Initializer != null)
            {
                value = evaluate(stmt.Initializer);
            }

            enviroment.define(stmt.name.lexeme, value);
            return null;
        }
        
        public object visitStmtWhileStmt(StmtWhile stmt)
        {
            while(isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.body);
            }
            return null;
        }

        public void executeBlock(List<Stmt> statements, Enviroment env)
        {
            Enviroment previous = enviroment;
            try
            {
                this.enviroment = env;

                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            } finally
            {
                this.enviroment = previous;
            }
        }
        #endregion

        #region EXPRESSIONS
        public object visitExprAssignExpr(ExprAssign expr)
        {
            object value = evaluate(expr.value);

            int distance;
            if (locals.TryGetValue(expr, out distance)) enviroment.assignAt(distance, expr.name, value);
            else globals.assign(expr.name, value);

            return value;
        }

        public object visitExprTernaryExpr(ExprTernary expr)
        {
            object top = evaluate(expr.top);

            switch (expr.topOperator.type)
            {
                case TokenType.QMARK:
                    if (isTruthy(top)) return evaluate(expr.left);
                    else return evaluate(expr.right);
                default:
                    return null;
            }
        }

        public object visitExprLogicalExpr(ExprLogical expr)
        {
            object left = evaluate(expr.left);

            switch (expr.eOperator.type) {
                case TokenType.OR:
                    if (isTruthy(left)) return left;
                    break;
                case TokenType.AND:
                    if (!isTruthy(left)) return left;
                    break;
                case TokenType.QMARK:
                    if (!isTruthy(left)) return null;
                    break;
            }
            return evaluate(expr.right);
        }

        public object visitExprBinaryExpr(ExprBinary expr)
        {
            object left = evaluate(expr.left);
            object right = evaluate(expr.right);

            switch (expr.eOperator.type)
            {
                case TokenType.GREATER:
                    checkNumberOperands(expr.eOperator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    checkNumberOperands(expr.eOperator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    checkNumberOperands(expr.eOperator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    checkNumberOperands(expr.eOperator, left, right);
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    checkNumberOperands(expr.eOperator, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    checkNumberOperands(expr.eOperator, left, right);
                    if ((double)right == 0)
                        throw new LoxRuntimeException(expr.eOperator, "Divider must not be zero.");
                    return (double)left / (double)right;
                case TokenType.STAR:
                    checkNumberOperands(expr.eOperator, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left.GetType() == typeof(double) && right.GetType() == typeof(double))
                    {
                        return (double)left + (double)right;
                    }
                    else if (left.GetType() == typeof(string) || right.GetType() == typeof(string))
                    {
                        return string.Concat(toString(left), toString(right));
                    }
                    else
                    {
                        throw new LoxRuntimeException(expr.eOperator, "Operands must be two numbers or two strings.");
                    }
                case TokenType.BANG_EQUAL:
                    return !left.Equals(right);
                case TokenType.EQUAL_EQUAL:
                    return left.Equals(right);

                case TokenType.COMMA:
                    return right;
                default:
                    return null;
            }
        }

        public object visitExprCallExpr(ExprCall expr)
        {
            object callee = evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(evaluate(argument));
            }

            if (!callee.GetType().GetInterfaces().Contains(typeof(ILoxCallable))) throw new LoxRuntimeException(expr.paren, "Can only call function and classes.");
            
            ILoxCallable function = (ILoxCallable)callee;
            if (function.arity() != arguments.Count)
                throw new LoxRuntimeException(expr.paren, "Expected " + 
                    function.arity() + " arguments but got " + arguments.Count + 
                    '.');
            return function.call(this, arguments);
        }

        public object visitExprGroupingExpr(ExprGrouping expr)
        {
            return evaluate(expr.expression);
        }

        public object visitExprLiteralExpr(ExprLiteral expr)
        {
            return expr.eValue;
        }

        public object visitExprUnaryExpr(ExprUnary expr)
        {
            object right = evaluate(expr.right);

            switch (expr.eOperator.type)
            {
                case TokenType.MINUS:
                    checkNumberOperand(expr.eOperator, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !isTruthy(right);
                default:
                    return null;
            }
        }

        public object visitExprVarExpr(ExprVar expr)
        {
            return lookUpVariable(expr.name, expr);
        }
        #endregion

        private bool isTruthy(object right)
        {
            if (right == null) return false;
            if (right.GetType() == typeof(bool)) return (bool)right;
            if (right.GetType() == typeof(double)) return (double)right != 0;
            return true;
        }

        string toString(object right)
        {
            if (right.GetType() == null) return "nil";
            if (right.GetType() == typeof(bool)) return (bool)right ? "true" : "false";
            return right.ToString();
        }

        void checkNumberOperands(Token o, params object[] operands)
        {
            try
            {
                foreach (object operand in operands)
                {
                    checkNumberOperand(o, operand);
                }
            }
            catch (LoxRuntimeException e)
            {
                throw new LoxRuntimeException(e.token, "Operands must be numbers");
            }
        }

        void checkNumberOperand(Token o, object operand)
        {
            if (operand.GetType() == typeof(double)) return;
            throw new LoxRuntimeException(o, "Operand must be a number.");
        }

        object evaluate(Expr expr)
        {
            return expr.accept(this);
        }

        public void resolve(Expr expr, int depth)
        {
            if (!locals.ContainsKey(expr))
                locals.Add(expr, 0);
            locals[expr] = depth;
        }

        object lookUpVariable(Token name, Expr expr)
        {
            int distance;
            return locals.TryGetValue(expr, out distance) ? enviroment.getAt(distance, name.lexeme) : globals.get(name);
        }
    }
}
