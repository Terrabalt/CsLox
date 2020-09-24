using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class ReturnValueNonException : ApplicationException
    {
        public object rValue;
        public ReturnValueNonException(object _rValue) : base ("This isn't an actual exception. If you see this, something in the interpreter has gone horribly wrong.")
        {
            rValue = _rValue;
        }
    }
}
