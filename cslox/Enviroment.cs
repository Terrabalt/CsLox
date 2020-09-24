using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Enviroment
    {
        Enviroment enclosing;
        Dictionary<string, object> values = new Dictionary<string, object>();

        public Enviroment()
        {
            enclosing = null;
        }

        public Enviroment(Enviroment closing)
        {
            enclosing = closing;
        }

        public object get(Token name)
        {
            object o;
            if (values.TryGetValue(name.lexeme, out o))
            {
                return o;
            }

            if (enclosing != null) return enclosing.get(name);

            throw new LoxRuntimeException(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public object getAt(int depth, string name)
        {
            object ret;
            if (!ancestor(depth).values.TryGetValue(name, out ret)) ret = null;
            return ret;
        }

        Enviroment ancestor(int distance)
        {
            Enviroment env = this;

            for (int i = 0; i < distance; i++)
            {
                env = env.enclosing;
            }

            return env;
        }
        
        public void define(string name, object value)
        {
            if (values.ContainsKey(name))
            {
                values[name] = value;
            } else
            {
                values.Add(name, value);
            }
        }

        internal void assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.assign(name, value);
                return;
            }

            throw new LoxRuntimeException(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public void assignAt(int distance, Token name, object v)
        {
            object ret;
            if (ancestor(distance).values.TryGetValue(name.lexeme, out ret))
                ancestor(distance).values[name.lexeme] = v;
            else
                ancestor(distance).values.Add(name.lexeme, v);
        }
    }
}
