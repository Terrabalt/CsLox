using System.Collections.Generic;
namespace cslox
{
    interface IExprVisitor<T> {
        object visitExprAssignExpr (ExprAssign expr);
        object visitExprTernaryExpr (ExprTernary expr);
        object visitExprLogicalExpr (ExprLogical expr);
        object visitExprBinaryExpr (ExprBinary expr);
        object visitExprCallExpr (ExprCall expr);
        object visitExprGroupingExpr (ExprGrouping expr);
        object visitExprLiteralExpr (ExprLiteral expr);
        object visitExprUnaryExpr (ExprUnary expr);
        object visitExprVarExpr (ExprVar expr);
    }
    abstract class Expr
    {
        public abstract object accept(IExprVisitor<object> visitor);
    }
    class ExprAssign : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprAssignExpr(this);
    }
        public Token name;
        public Expr value;
        public ExprAssign (Token _name, Expr _value)
        {
            name = _name;
            value = _value;
        }
    }
    class ExprTernary : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprTernaryExpr(this);
    }
        public Expr top;
        public Token topOperator;
        public Expr left;
        public Token eOperator;
        public Expr right;
        public ExprTernary (Expr _top, Token _topOperator, Expr _left, Token _eOperator, Expr _right)
        {
            top = _top;
            topOperator = _topOperator;
            left = _left;
            eOperator = _eOperator;
            right = _right;
        }
    }
    class ExprLogical : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprLogicalExpr(this);
    }
        public Expr left;
        public Token eOperator;
        public Expr right;
        public ExprLogical (Expr _left, Token _eOperator, Expr _right)
        {
            left = _left;
            eOperator = _eOperator;
            right = _right;
        }
    }
    class ExprBinary : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprBinaryExpr(this);
    }
        public Expr left;
        public Token eOperator;
        public Expr right;
        public ExprBinary (Expr _left, Token _eOperator, Expr _right)
        {
            left = _left;
            eOperator = _eOperator;
            right = _right;
        }
    }
    class ExprCall : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprCallExpr(this);
    }
        public Expr callee;
        public Token paren;
        public List<Expr> arguments;
        public ExprCall (Expr _callee, Token _paren, List<Expr> _arguments)
        {
            callee = _callee;
            paren = _paren;
            arguments = _arguments;
        }
    }
    class ExprGrouping : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprGroupingExpr(this);
    }
        public Expr expression;
        public ExprGrouping (Expr _expression)
        {
            expression = _expression;
        }
    }
    class ExprLiteral : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprLiteralExpr(this);
    }
        public object eValue;
        public ExprLiteral (object _eValue)
        {
            eValue = _eValue;
        }
    }
    class ExprUnary : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprUnaryExpr(this);
    }
        public Token eOperator;
        public Expr right;
        public ExprUnary (Token _eOperator, Expr _right)
        {
            eOperator = _eOperator;
            right = _right;
        }
    }
    class ExprVar : Expr
    {

    public override object accept(IExprVisitor<object> visitor) {
      return visitor.visitExprVarExpr(this);
    }
        public Token name;
        public ExprVar (Token _name)
        {
            name = _name;
        }
    }
}
