using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Scanner
    {
        private string source;
        private List<Token> tokens;
        private int start, current, line;
        private static Dictionary<string, TokenType> keywords;
        

        public Scanner(string source)
        {
            this.source = source;
            tokens = new List<Token>();
            start = 0;
            current = 0;
            line = 1;

            if (keywords == null)
            {
                keywords = new Dictionary<string, TokenType>();
                keywords.Add("and", TokenType.AND);
                keywords.Add("class", TokenType.CLASS);
                keywords.Add("else", TokenType.ELSE);
                keywords.Add("false", TokenType.FALSE);
                keywords.Add("for", TokenType.FOR);
                keywords.Add("fun", TokenType.FUN);
                keywords.Add("if", TokenType.IF);
                keywords.Add("nil", TokenType.NIL);
                keywords.Add("or", TokenType.OR);
                keywords.Add("print", TokenType.PRINT);
                keywords.Add("return", TokenType.RETURN);
                keywords.Add("super", TokenType.SUPER);
                keywords.Add("this", TokenType.THIS);
                keywords.Add("true", TokenType.TRUE);
                keywords.Add("var", TokenType.VAR);
                keywords.Add("while", TokenType.WHILE);

            }
        }

        public List<Token> scanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                scanToken();
            }

            tokens.Add(new cslox.Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void scanToken()
        {
            char c = advance();
            switch (c)
            {
                case '(': addToken(TokenType.LEFT_PAREN); break;
                case ')': addToken(TokenType.RIGHT_PAREN); break;
                case '{': addToken(TokenType.LEFT_BRACE); break;
                case '}': addToken(TokenType.RIGHT_BRACE); break;
                case ',': addToken(TokenType.COMMA); break;
                case '.': addToken(TokenType.DOT); break;
                case '-': addToken(TokenType.MINUS); break;
                case '+': addToken(TokenType.PLUS); break;
                case ';': addToken(TokenType.SEMICOLON); break;
                case '*': addToken(TokenType.STAR); break;

                case '?': addToken(TokenType.QMARK); break;
                case ':': addToken(TokenType.COLON); break;

                case '!': addToken(matchNext('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': addToken(matchNext('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': addToken(matchNext('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': addToken(matchNext('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/':
                    if (matchNext('/'))
                    {
                        // A comment goes until the end of a line
                        while (peek() != '\n' && !IsAtEnd()) advance();
                    }
                    else if(matchNext('*'))
                    {
                        // A comment block
                        bool isStop = false;
                        while (!isStop && !IsAtEnd())
                        {
                            if (peek() == '\n') line++;
                            if ((peek() == '*' && peekNext() == '/'))
                            {
                                isStop = true;
                                advance();
                            }
                            advance();
                        }
                        if (!isStop)
                        {
                            Program.error(line, "Undetermined comment block.");
                        }
                    }
                    else
                    {
                        addToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break; 
                case '\n':
                    line++;
                    break;
                case '"': scanString(); break; 
                default:
                    if (IsDigit(c))
                    {
                        number();
                    } else if (IsAlpha(c))
                    {
                        identifier();
                    }
                    else
                    {
                        Program.error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                    c == '_';
        }


        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void identifier()
        {
            while (IsAlphaNumeric(peek())) advance();
            string text = source.Substring(start, current - start);

            TokenType type;
            if (!keywords.TryGetValue(text, out type)) type = TokenType.IDENTIFIER;
            addToken(type);
        }

        private void number()
        {
            while (IsDigit(peek())) advance();

            if(peek() == '.' && IsDigit(peekNext()))
            {
                advance();
                while (IsDigit(peek())) advance();
            }

            addToken(TokenType.NUMBER, double.Parse(source.Substring(start, current - start),System.Globalization.NumberStyles.AllowDecimalPoint));
        }

        private char peekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void scanString()
        {
            while (peek() != '"' && !IsAtEnd())
            {
                if (peek() == '\n') line++;
                advance();
            }

            // Undetermined string
            if(IsAtEnd())
            {
                Program.error(line, "Undetermined string.");
                return;
            }
            
            // The closing quote
            if(peek() == '"')
            {
                advance();

                // Trim the surrounding quotes.
                String val = source.Substring(start + 1, current - (start + 2) );
                addToken(TokenType.STRING, val);
            }    
        }

        private char peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        private bool matchNext(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private void addToken(TokenType type)
        {
            addToken(type, null);
        }

        private void addToken(TokenType type, object literal)
        {
            String text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        private char advance()
        {
            current++;
            return source[current - 1];
        }

        private bool IsAtEnd()
        {
            return current >= source.Length;
        }
    }
}
