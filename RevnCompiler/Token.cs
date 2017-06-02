﻿using System;
namespace RevnCompiler
{
	public enum TokenType
	{
		Identifier,
        Integer,
        FloatingPoint,
		Predefined,

        Using,
        Namespace,
        Class,

        LParen,
        RParen,
        LBracket,
        RBracket,
        BlockStartOrColon,
        BlockEnd,
        Period,
        Comma,
        Equals,

        Val,
        Var,

        Accessibility,

        Static,

        Fun,

        StringLiteral,
        Number,
	}

	public class Token
	{
        public TokenType TokenType { get; }
		public string Value { get; }
		public int LineNumber { get; }

		internal Token(TokenType tokenType, string val, int lineNumber)
		{
			TokenType = tokenType;
			Value = val;
			LineNumber = lineNumber;
		}
	}
}
