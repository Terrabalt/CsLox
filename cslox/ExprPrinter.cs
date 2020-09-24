using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class ExprPrinter : IExprVisitor<object>
    {
        public String print(Expr expr)
        {
            return (string)expr.accept(this);
        }

        public object visitExprAssignExpr(ExprAssign expr)
        {
            throw new NotImplementedException();
        }

        public object visitExprLogicalExpr(ExprLogical expr)
        {
            return parenthesize(expr.eOperator.lexeme, expr.left, expr.right);
        }

        public object visitExprTernaryExpr(ExprTernary expr)
        {
            return parenthesize(expr.eOperator.lexeme + ' ' + expr.eOperator.lexeme, expr.top, expr.left, expr.right);
        }

        public object visitExprBinaryExpr(ExprBinary expr)
        {
            return parenthesize(expr.eOperator.lexeme, expr.left, expr.right);
        }

        public object visitExprCallExpr(ExprCall expr)
        {
            throw new NotImplementedException();
        }
        public object visitExprGroupingExpr(ExprGrouping expr)
        {
            return parenthesize("group", expr.expression);
        }
        public object visitExprLiteralExpr(ExprLiteral expr)
        {
            if (expr.eValue == null) return "Nil";
            return expr.eValue.ToString();
        }
        public object visitExprUnaryExpr(ExprUnary expr)
        {
            return parenthesize(expr.eOperator.lexeme, expr.right);
        }
        public object visitExprVarExpr(ExprVar expr)
        {
            return expr.name;
        }
        public string parenthesize(string name, params Expr[] exprs)
        {
            String a = '(' + name;
            foreach (Expr expr in exprs)
            {
                a += ' ' + (string)expr.accept(this);
            }
            a += ')';
            return a;
        }
    }
    class PolishNotationExprPrinter : IExprVisitor<object>
    {
        public String print(Expr expr)
        {
            return (string)expr.accept(this);
        }

        public object visitExprAssignExpr(ExprAssign expr)
        {
            throw new NotImplementedException();
        }

        public object visitExprTernaryExpr(ExprTernary expr)
        {
            return parenthesize(expr.eOperator.lexeme + ' ' + expr.eOperator.lexeme, expr.top, expr.left, expr.right);
        }

        public object visitExprLogicalExpr(ExprLogical expr)
        {
            return parenthesize(expr.eOperator.lexeme, expr.left, expr.right);
        }

        public object visitExprBinaryExpr(ExprBinary expr)
        {
            return parenthesize(expr.eOperator.lexeme, expr.left, expr.right);
        }
        public object visitExprGroupingExpr(ExprGrouping expr)
        {
            return (string)expr.expression.accept(this);
        }
        public object visitExprCallExpr(ExprCall expr)
        {
            throw new NotImplementedException();
        }
        public object visitExprLiteralExpr(ExprLiteral expr)
        {
            if (expr.eValue == null) return "Nil";
            return expr.eValue.ToString();
        }
        public object visitExprVarExpr(ExprVar expr)
        {
            return expr.name;
        }
        public object visitExprUnaryExpr(ExprUnary expr)
        {
            return parenthesize(expr.eOperator.lexeme, expr.right);
        }
        public string parenthesize(string name, params Expr[] exprs)
        {
            String a = "";
            foreach (Expr expr in exprs)
            {
                a += (string)expr.accept(this) + ' ';
            }
            a += name;
            return a;
        }
    }
}
