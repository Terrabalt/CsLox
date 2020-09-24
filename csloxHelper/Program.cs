using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csloxHelper
{
    class Program
    { 
        static void Main(string[] args)
        {
            if (args.Length != 1)
                Console.WriteLine("Usage: csloxHelper <output directory>");
            else
            {
                String outputDir = args[0];
                string[] syntaxTree = {
                    "Assign : Token name, Expr value",
                    "Ternary : Expr top, Token topOperator, Expr left, Token eOperator, Expr right",
                    "Logical : Expr left, Token eOperator, Expr right",
                    "Binary : Expr left, Token eOperator, Expr right",
                    "Call : Expr callee, Token paren, List<Expr> arguments",
                    "Grouping : Expr expression",
                    "Literal : object eValue",
                    "Unary : Token eOperator, Expr right",
                    "Var : Token name"
                };

                defineExprs(outputDir, "Expr", syntaxTree);

                syntaxTree = new string[] {
                    "Block : List<Stmt> statements",
                    "Expression : Expr expression",
                    "Function : Token name, List<Token> arguments, List<Stmt> body",
                    "If : Expr condition, Stmt thenBranch, Stmt elseBranch",
                    "Print : Expr expression",
                    "Return : Token keyword, Expr rValue",
                    "Var : Token name, Expr Initializer",
                    "While : Expr condition, Stmt body"
                };
                defineExprs(outputDir, "Stmt", syntaxTree);
            }
        }

        static void defineVisitor(System.IO.StreamWriter writer, string baseName, string[] types)
        {
            writer.WriteLine("    interface I" + baseName + "Visitor<T> {");
            string typeName;
            foreach (string type in types)
            {
                typeName = baseName + type.Split(':')[0].Trim();
                writer.WriteLine("        object visit" + typeName + baseName + " (" + typeName + ' ' + baseName.ToLower() + ");");
            }
            writer.WriteLine("    }");
        }

        static void defineExprs(string outputDir, string baseName, string[] types)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputDir + "\\" + baseName + ".cs", false))
            {
                file.WriteLine("using System.Collections.Generic;");
                file.WriteLine("namespace cslox");
                file.WriteLine("{");
                defineVisitor(file, baseName, types);
                file.WriteLine("    abstract class " + baseName);
                file.WriteLine("    {");
                file.WriteLine("        public abstract object accept(I" + baseName + "Visitor<object> visitor);");
                file.WriteLine("    }");

                string className;
                string[] fields;
                foreach (string type in types)
                {
                    className = baseName + type.Split(':')[0].Trim();
                    fields = type.Split(':')[1].Split(',');
                    file.WriteLine("    class " + className + " : " + baseName);
                    file.WriteLine("    {");
                    // Visitor pattern.                                      
                    file.WriteLine();
                    file.WriteLine("    public override object accept(I"+baseName+"Visitor<object> visitor) {");
                    file.WriteLine("      return visitor.visit" +
                        className + baseName + "(this);");
                    file.WriteLine("    }");
                    foreach (string field in fields)
                    {
                        file.WriteLine("        public " + field.Trim() + ";");
                    }
                    file.Write("        public " + className + " (");
                    for (int i = 0; i < fields.Length; i++)
                    {
                        string field = fields[i];
                        file.Write(field.Trim().Split(' ')[0] + " _" + field.Trim().Split(' ')[1]);
                        if (i + 1 != fields.Length)
                        {
                            file.Write(", ");
                        }
                    }
                    file.WriteLine(")");
                    file.WriteLine("        {");
                    foreach (string field in fields)
                    {
                        string x = field.Trim().Split(' ')[1];
                        file.WriteLine("            " + x + " = _" + x + ";" );
                    }
                    file.WriteLine("        }");
                    file.WriteLine("    }");

                }
                file.WriteLine("}");
            }
        }
    }
}
