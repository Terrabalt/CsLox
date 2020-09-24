using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class LoxFunction : ILoxCallable
    {
        StmtFunction declaration;
        Enviroment closure;

        public LoxFunction(StmtFunction _declaration, Enviroment _closure)
        {
            closure = _closure;
            declaration = _declaration;
        }

        public int arity()
        {
            return declaration.arguments.Count;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            Enviroment enviroment = new Enviroment(closure);
            for (int i = 0; i < declaration.arguments.Count; i++)
            {
                enviroment.define(declaration.arguments[i].lexeme, arguments[i]);
            }

            try
            {
                interpreter.executeBlock(declaration.body, enviroment);
            } catch (ReturnValueNonException e)
            {
                return e.rValue;
            }
            return null;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + '>';
        }
    }

    class NativeClockFunc : ILoxCallable
    {
        public int arity()
        {
            return 0;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            return ((double)Environment.TickCount) / 1000.0;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
