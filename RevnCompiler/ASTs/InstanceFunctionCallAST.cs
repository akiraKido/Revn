using System;
using System.Collections.Generic;
namespace RevnCompiler.ASTs
{
    internal class InstanceFunctionCallAST : ExpressionAST
    {
        internal string instanceName;
        internal string callFunction;
        internal List<ExpressionAST> args;

        public override string GenerateIL()
        {
            throw new NotImplementedException();
        }
    }
}
