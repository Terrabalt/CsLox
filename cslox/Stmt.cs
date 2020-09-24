using System.Collections.Generic;
namespace cslox
{
    interface IStmtVisitor<T> {
        object visitStmtBlockStmt (StmtBlock stmt);
        object visitStmtExpressionStmt (StmtExpression stmt);
        object visitStmtFunctionStmt (StmtFunction stmt);
        object visitStmtIfStmt (StmtIf stmt);
        object visitStmtPrintStmt (StmtPrint stmt);
        object visitStmtReturnStmt (StmtReturn stmt);
        object visitStmtVarStmt (StmtVar stmt);
        object visitStmtWhileStmt (StmtWhile stmt);
    }
    abstract class Stmt
    {
        public abstract object accept(IStmtVisitor<object> visitor);
    }
    class StmtBlock : Stmt
    {

    public override object accept(IStmtVisitor<object> visitor) {
      return visitor.visitStmtBlockStmt(this);
    }
        public List<Stmt> statements;
        public StmtBlock (List<Stmt> _statements)
        {
            statements = _statements;
        }
    }
    class StmtExpression : Stmt
    {

    public override object accept(IStmtVisitor<object> visitor) {
      return visitor.visitStmtExpressionStmt(this);
    }
        public Expr expression;
        public StmtExpression (Expr _expression)
        {
            expression = _expression;
        }
    }
    class StmtFunction : Stmt
    {

    public override object accept(IStmtVisitor<object> visitor) {
      return visitor.visitStmtFunctionStmt(this);
    }
        public Token name;
        public List<Token> arguments;
        public List<Stmt> body;
        public StmtFunction (Token _name, List<Token> _arguments, List<Stmt> _body)
        {
            name = _name;
            arguments = _arguments;
            body = _body;
        }
    }
    class StmtIf : Stmt
    {

    public override object accept(IStmtVisitor<object> visitor) {
      return visitor.visitStmtIfStmt(this);
    }
        public Expr condition;
        public Stmt thenBranch;
        public Stmt elseBranch;
        public StmtIf (Expr _condition, Stmt _thenBranch, Stmt _elseBranch)
        {
            condition = _condition;
            thenBranch = _thenBranch;
            elseBranch = _elseBranch;
        }
    }
    class StmtPrint : Stmt
    {

    public override object accept(IStmtVisitor<object> visitor) {
      return visitor.visitStmtPrintStmt(this);
    }
        public Expr expression;
        public StmtPrint (Expr _expression)
        {
            expression = _expression;
        }
    }
    class StmtReturn : Stmt
    {

    public override object accept(IStmtVisitor<object> visitor) {
      return visitor.visitStmtReturnStmt(this);
    }
        public Token keyword;
        public Expr rValue;
        public StmtReturn (Token _keyword, Expr _rValue)
        {
            keyword = _keyword;
            rValue = _rValue;
        }
    }
    class StmtVar : Stmt
    {

    public override object accept(IStmtVisitor<object> visitor) {
      return visitor.visitStmtVarStmt(this);
    }
        public Token name;
        public Expr Initializer;
        public StmtVar (Token _name, Expr _Initializer)
        {
            name = _name;
            Initializer = _Initializer;
        }
    }
    class StmtWhile : Stmt
    {

    public override object accept(IStmtVisitor<object> visitor) {
      return visitor.visitStmtWhileStmt(this);
    }
        public Expr condition;
        public Stmt body;
        public StmtWhile (Expr _condition, Stmt _body)
        {
            condition = _condition;
            body = _body;
        }
    }
}
