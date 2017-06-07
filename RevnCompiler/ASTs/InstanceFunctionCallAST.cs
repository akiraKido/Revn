using System;
using System.Collections.Generic;
namespace RevnCompiler.ASTs
{
    internal class InstanceFunctionCallAST : ExpressionAST
    {
        internal VariableExpressionAST Variable;
        internal string CallFunction;
        internal List<ExpressionAST> Args;

        public override string GenerateIL()
        {
            string argLoad = string.Empty;
            string argLine = string.Empty;
            foreach (var arg in Args)
            {
                argLoad += arg.GenerateIL();
                argLine += arg.ReturnType + ",";
            }
            if (Args.Count > 0)
            {
                // remove final ,
                argLine = argLine.Substring(0, argLine.Length - 1);
            }

            string IL = string.Empty;
            IL += Variable.GenerateIL();
            IL += argLoad;

            // TODO: get Retval and arg from function
            IL += $"callvirt instance int32 {Variable.ReturnType}::{CallFunction}({argLine})\n";
            return IL;
        }
    }
}
