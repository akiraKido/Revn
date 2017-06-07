﻿namespace RevnCompiler.ASTs
{
    internal class VariableExpressionAST : ExpressionAST
    {
        internal string Name { get; set; }
        internal int Index { get; set; }
        internal bool IsMutable { get; set; }
        internal bool IsToSet { get; set; }

        public override string GenerateIL()
        {
            return IsToSet
                ? $"stloc.{Index} //{Name}\n"
                : $"ldloc.{Index} //{Name}\n";
        }
    }
}
