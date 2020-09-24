using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class LoxRuntimeException : ApplicationException
    {
        public Token token;

        public LoxRuntimeException(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}
