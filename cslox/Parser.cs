using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Parser
    {
        private List<Token> tokens;
        int current = 0;

        public Parser(List<Token> _tokens)
        {
            tokens = _tokens;
        }

        public List<Stmt> parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!isAtEnd())
            {
                statements.Add(declaration());
            }
            return statements;
        }

        Stmt declaration()
        {
            try
            {
                if (match(TokenType.VAR)) return varDeclaration();

                return statement();
            }
            catch (ParseError)
            {
                synchronize();
                return null;
            }
        }

        Stmt varDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expected variable name");

            Expr initializer = null;
            if (match(TokenType.EQUAL))
            {
                initializer = expression();
            }

            consume(TokenType.SEMICOLON, "Expected ';' after value.");
            return new StmtVar(name, initializer);
        }

        Stmt statement()
        {
            if (match(TokenType.FUN)) return funcStatement("function");
            if (match(TokenType.FOR)) return forStatement();
            if (match(TokenType.IF)) return ifStatement();
            if (match(TokenType.PRINT)) return printStatement();
            if (match(TokenType.RETURN)) return returnStatement();
            if (match(TokenType.WHILE)) return whileStatement();
            if (match(TokenType.LEFT_BRACE)) return new StmtBlock(block());

            return exprStatement();
        }

        List<Stmt> block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!check(TokenType.RIGHT_BRACE) && !isAtEnd())
            {
                statements.Add(declaration());
            }

            consume(TokenType.RIGHT_BRACE, "Expected '}' after block");
            return statements;
        }

        StmtFunction funcStatement(String kind)
        {
            Token name = consume(TokenType.IDENTIFIER, "Expected " + kind + " name.");
            consume(TokenType.LEFT_PAREN, "Expected '(' after " + kind + " name");

            List<Token> arguments = new List<Token>();
            if (!check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        error(peek(), "Cannot have more than 255 parameters.");
                    }
                    arguments.Add(consume(TokenType.IDENTIFIER, "Expected parameter name."));
                } while (match(TokenType.COMMA));
            }
            consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments.");

            List<Stmt> body;
            if (match(TokenType.SEMICOLON))
            {
                body = new List<Stmt>();
            }
            else
            {
                consume(TokenType.LEFT_BRACE, "Expected '{' or ';' after " + kind + " head.");
                body = block();
            }

            return new StmtFunction(name, arguments, body);
        }

        Stmt forStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expected '(' after 'for'.");
            Stmt initStmt;
            if (match(TokenType.SEMICOLON)) initStmt = null;
            else if (match(TokenType.VAR)) initStmt = varDeclaration();
            else initStmt = exprStatement();

            Expr condition = null;
            if (!check(TokenType.SEMICOLON))
            {
                condition = expression();
            }
            consume(TokenType.SEMICOLON, "Expected ';' after loop condition");

            Expr increment = null;
            if (!check(TokenType.RIGHT_PAREN))
            {
                increment = expression();
            }
            consume(TokenType.RIGHT_PAREN, "Expected ')' after for clauses");

            Stmt body = statement();

            if (increment != null)
                body = new StmtBlock(new List<Stmt>() { body, new StmtExpression(increment) });
            if (condition == null)
                condition = new ExprLiteral(true);
            body = new StmtWhile(condition, body);
            if (initStmt != null)
                body = new StmtBlock(new List<Stmt>() { initStmt, body });

            return body;
        }

        Stmt ifStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expected '(' after 'if'.");
            Expr condExpr = expression();
            consume(TokenType.RIGHT_PAREN, "Expected ')' after if condition.");
            Stmt thenStmt = statement();
            Stmt elseStmt = null;
            if (match(TokenType.ELSE)) elseStmt = statement();
            return new StmtIf(condExpr, thenStmt, elseStmt);
        }

        Stmt printStatement()
        {
            Expr v = expression();
            consume(TokenType.SEMICOLON, "Expected ';' after value.");
            return new StmtPrint(v);
        }

        Stmt returnStatement()
        {
            Token keyword = previous();
            Expr value = null;
            bool hasValue = false;
            if(!check(TokenType.SEMICOLON))
            {
                hasValue = true;
                value = commaSeparatedExprs();
            }

            consume(TokenType.SEMICOLON, "Expected ';' after return " + (hasValue ? "value" : "statement") + '.');
            return new StmtReturn(keyword, value);
        }

        Stmt whileStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expected '(' after 'if'.");
            Expr condExpr = expression();
            consume(TokenType.RIGHT_PAREN, "Expected ')' after if condition.");
            Stmt bodyStmt = statement();

            return new StmtWhile(condExpr, bodyStmt);
        }

        Stmt exprStatement()
        {
            Expr v = commaSeparatedExprs();
            consume(TokenType.SEMICOLON, "Expected ';' after value.");
            return new StmtExpression(v);
        }

        private Expr commaSeparatedExprs()
        {
            if (match(TokenType.COMMA))
            {
                Token operatorExpr = previous();
                Expr right = expression();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = expression();

            while (match(TokenType.COMMA))
            {
                Token operatorExpr = previous();
                Expr right = ternaryConditionalExpr();
                expr = new ExprBinary(expr, operatorExpr, right);
            }
            return expr;
        }

        private Expr expression()
        {
            return ternaryConditionalExpr();
        }
        
        private Expr ternaryConditionalExpr()
        {
            if (match(TokenType.QMARK))
            {
                Token operatorExpr = previous();
                Expr right = assignmentExpr();
                if (match(TokenType.COLON)) right = assignmentExpr();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = assignmentExpr();

            if (match(TokenType.QMARK))
            {
                Token operatorExpr1 = previous();
                Expr leftExpr = assignmentExpr();
                if (match(TokenType.COLON))
                {
                    Token operatorExpr2 = previous();
                    Expr rightExpr = assignmentExpr();
                    expr = new ExprTernary(expr, operatorExpr1, leftExpr, operatorExpr2, rightExpr);
                }
                else expr = new ExprLogical(expr, operatorExpr1, leftExpr);
            }
            return expr;
        }

        private Expr assignmentExpr()
        {
            if (match(TokenType.EQUAL))
            {
                Token operatorExpr = previous();
                Expr right = logicAndExpr();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = logicAndExpr();

            if (match(TokenType.EQUAL))
            {
                Token equals = previous();
                Expr value = assignmentExpr();

                if (expr.GetType() == typeof(ExprVar))
                {
                    Token name = ((ExprVar)expr).name;
                    return new ExprAssign(name, value);
                }
                error(equals, "Invalid assignment target.");
            }
            return expr;
        }

        Expr logicOrExpr()
        {
            if (match(TokenType.OR))
            {
                Token operatorExpr = previous();
                Expr right = logicAndExpr();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = logicAndExpr();

            while(match(TokenType.OR))
            {
                Token op = previous();
                Expr right = logicAndExpr();
                expr = new ExprLogical(expr, op, right);
            }
            return expr;
        }

        Expr logicAndExpr()
        {
            if (match(TokenType.AND))
            {
                Token operatorExpr = previous();
                Expr right = equalityExpr();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = equalityExpr();

            while (match(TokenType.AND))
            {
                Token op = previous();
                Expr right = equalityExpr();
                expr = new ExprLogical(expr, op, right);
            }
            return expr;
        }

        private Expr equalityExpr()
        {
            if (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token operatorExpr = previous();
                Expr right = comparisonExpr();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = comparisonExpr();
            while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token operatorExpr = previous();
                Expr right = comparisonExpr();
                expr = new ExprBinary(expr, operatorExpr, right);
            }
            return expr;
        }

        private Expr comparisonExpr()
        {
            if (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token operatorExpr = previous();
                Expr right = additionExpr();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = additionExpr();

            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token operatorExpr = previous();
                Expr right = additionExpr();
                expr = new ExprBinary(expr, operatorExpr, right);
            }

            return expr;
        }

        private Expr additionExpr()
        {
            if (match(TokenType.MINUS, TokenType.PLUS))
            {
                Token operatorExpr = previous();
                Expr right = multiplicationExpr();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = multiplicationExpr();

            while (match(TokenType.MINUS, TokenType.PLUS))
            {
                Token operatorExpr = previous();
                Expr right = multiplicationExpr();
                expr = new ExprBinary(expr, operatorExpr, right);
            }

            return expr;
        }

        private Expr multiplicationExpr()
        {
            if (match(TokenType.STAR, TokenType.SLASH))
            {
                Token operatorExpr = previous();
                Expr right = unaryExpr();
                throw error(operatorExpr, "Expected expression before '" + operatorExpr.lexeme + "'.");
            }
            Expr expr = unaryExpr();

            while (match(TokenType.STAR, TokenType.SLASH))
            {
                Token operatorExpr = previous();
                Expr right = unaryExpr();
                expr = new ExprBinary(expr, operatorExpr, right);
            }

            return expr;
        }

        private Expr unaryExpr()
        {
            if (match(TokenType.BANG, TokenType.MINUS))
            {
                Token operatorExpr = previous();
                Expr right = unaryExpr();
                return new ExprUnary(operatorExpr, right);
            }

            return callExpr();
        }

        private Expr callExpr()
        {
            Expr expr = primaryExpr();

            bool x = true;
            while (x)
            {
                if (match(TokenType.LEFT_PAREN))
                {
                    expr = finishCall(expr);
                }
                else
                {
                    x = false;
                }
            }

            return expr;
        }

        private Expr finishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if(!check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        error(peek(), "Cannot have more than 255 arguments.");
                    }
                    arguments.Add(expression());
                } while (match(TokenType.COMMA));
            }
            Token paren = consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments.");

            return new ExprCall(callee, paren, arguments);
        }

        private Expr primaryExpr()
        {
            if (match(TokenType.FALSE)) return new ExprLiteral(false);
            if (match(TokenType.TRUE)) return new ExprLiteral(true);
            if (match(TokenType.NIL)) return new ExprLiteral(null);

            if (match(TokenType.NUMBER, TokenType.STRING)) return new ExprLiteral(previous().literal);

            if (match(TokenType.IDENTIFIER)) return new ExprVar(previous());

            if (match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
                return new ExprGrouping(expr);
            }

            throw error(peek(), "Expected expression");
        }

        private Token consume(TokenType type, string v)
        {
            if (check(type)) return advance();

            throw new Exception(v);
        }

        private ParseError error(Token token, string message)
        {
            Program.error(token, message);

            return new ParseError();
        }

        void synchronize()
        {
            advance();

            while (!isAtEnd())
            {
                if (previous().type == TokenType.SEMICOLON) return;

                switch (peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }


                advance();
            }
        }

        private bool match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (check(type))
                {
                    advance();
                    return true;
                }
            }
            return false;
        }

        private Token advance()
        {
            if (!isAtEnd()) current++;
            return previous();
        }

        private bool check(TokenType type)
        {
            if (isAtEnd()) return false;
            return peek().type == type;
        }

        bool isAtEnd()
        {
            return peek().type == TokenType.EOF;
        }

        Token peek()
        {
            return tokens[current];
        }

        Token previous()
        {
            return tokens[current - 1];
        }
    }

    class ParseError : Exception
    {

    }
}
