﻿using System;
using System.Collections.Generic;
using RevnCompiler.ASTs;

namespace RevnCompiler.ParserHelpers
{
    internal class ExpressionASTGenerator : IExpressionASTGenerator
    {
        private readonly Dictionary<string, int> BinopPrecedence = new Dictionary<string, int>()
        {
            {"<", 10},
            {"+", 20},
            {"-", 20},
            {"*", 40},
            {"=", 100}
        };

        private readonly Parser parser;
        private readonly FunctionASTGenerator functionGenerator;
        private int localVariableIndex;
        private int LocalVariableIndex => localVariableIndex++;

        internal ExpressionASTGenerator(Parser parser, FunctionASTGenerator functionGenerator)
        {
            this.parser = parser;
            this.functionGenerator = functionGenerator;
        }

        public ExpressionAST GenerateExpressionAST()
        {
            var LHS = ParsePrimary();
            return ParseBinOpRHS(0, LHS);
        }

        private ExpressionAST ParsePrimary()
        {
            switch(parser.LastToken.TokenType)
            {
                case TokenType.Identifier:
                    return ParseIdentifier(null);
                case TokenType.StringLiteral:
                    var returnStringLiteral = new StringLiteralAST(parser.LastToken.Value);
                    parser.ProceedToken(); // 文字列を消費
                    return returnStringLiteral;
                case TokenType.Var:
                case TokenType.Val:
                    return ParseVariableAssignment();
                case TokenType.Integer:
                    var intVal = new IntegerLiteralAST(parser.LastToken.Value);
                    parser.ProceedToken(); // 数字を消費
                    return intVal;
                case TokenType.FloatingPoint:
                    var floatVal = new FloatLiteralAST(parser.LastToken.Value);
                    parser.ProceedToken(); // 数字を消費
                    return floatVal;
                default:
                    throw new NotImplementedException();
            }
        }

        private ExpressionAST ParseVariableAssignment()
        {
            var variable = new VariableExpressionAST();
            variable.IsToSet = true;
            variable.IsMutable = parser.LastToken.TokenType == TokenType.Var;

            parser.ProceedToken(); // val/var を消費
            Assert.AssertTypeMatch(parser.LastToken, TokenType.Identifier);
            variable.Name = parser.LastToken.Value;

            parser.ProceedToken(); // 変数名を消費

            if(parser.LastToken.TokenType == TokenType.BlockStartOrColon)
            {
                parser.ProceedToken(); // : を消費
                Assert.AssertTypeMatch(parser.LastToken, TokenType.Identifier);
                variable.ReturnType = parser.LastToken.Value;
                parser.ProceedToken(); // 型を消費
            }

            if(parser.LastToken.TokenType != TokenType.Equals)
            {
                if(variable.ReturnType == null)
                {
                    RevnException.ThrowParserException($"Variable {variable.Name} must have a type.", parser.LastToken);
                }
                functionGenerator.AddVariable(variable);
                return variable;
            }

            parser.ProceedToken(); // = を消費

            var assignment = new AssignmentAST
            {
                LHS = variable,
                RHS = GenerateExpressionAST()
            };

            if(variable.ReturnType == null)
            {
                variable.ReturnType = assignment.RHS.ReturnType;
            }
            functionGenerator.AddVariable(variable);

            return assignment;
        }

        private ExpressionAST ParseIdentifier(string inferedType)
        {
            string identifier = parser.LastToken.Value;
            parser.ProceedToken();

            while(parser.LastToken.TokenType == TokenType.Period)
            {
                parser.ProceedToken(); // . を消費
                Assert.AssertTypeMatch(parser.LastToken, TokenType.Identifier);
                identifier += "." + parser.LastToken.Value;
                parser.ProceedToken(); // 変数を消費
            }

            if(parser.LastToken.TokenType != TokenType.LParen)
            {
                if (!functionGenerator.HasLocalVariable(identifier))
                {
                    RevnException.ThrowParserException("Variable is not assigned.", parser.LastToken);
                }

                if(parser.LastToken.TokenType == TokenType.Identifier)
                {
                    string functionName = parser.LastToken.Value;
                    parser.ProceedToken(); // 関数名を消費
                    var arg = new List<ExpressionAST>();
                    arg.Add(GenerateExpressionAST());

                    var instanceCall = new InstanceFunctionCallAST
                    {
                        Variable = ParseVariable(identifier),
                        CallFunction = functionName,
                        Args = arg
                    };

                    return instanceCall;
                }

                // return variable
                return ParseVariable(identifier);
            }

            if (inferedType == null)
            {
                inferedType = "void";
            }

            parser.ProceedToken(); // ( を消費

            var args = new List<ExpressionAST>();
            if(parser.LastToken.TokenType != TokenType.RParen)
            {
                while (true)
                {
                    var arg = GenerateExpressionAST();
                    args.Add(arg);

                    if (parser.LastToken.TokenType == TokenType.RParen)
                        break;

                    if (parser.LastToken.TokenType != TokenType.Comma)
                    {
                        RevnException.ThrowParserException("Expected comma or )", parser.LastToken);
                    }
                    parser.ProceedToken(); // , を消費
                }
            }

            parser.ProceedToken(); // ) を消費

            return new CallExpressionAST(identifier, args, inferedType);
        }

        private VariableExpressionAST ParseVariable(string identifier)
        {
            string type = functionGenerator.GetVariable(identifier).ReturnType;

            var variable = new VariableExpressionAST();
            variable.ReturnType = type;
            variable.Name = identifier;
            variable.Index = LocalVariableIndex;
            return variable;
        }

        private ExpressionAST ParseBinOpRHS(int expressionPrecedence, ExpressionAST LHS)
        {
            while(true)
            {
                int tokenPrecedence = GetPrecedence(parser.LastToken);
                if (tokenPrecedence < expressionPrecedence) return LHS;

                // operator can be any string
                string _operator = parser.LastToken.Value;
                parser.ProceedToken(); // 演算子を消費

                var RHS = ParsePrimary();
                RHS.IsReturnValueUsed = true;

                int nextPrecedence = GetPrecedence(parser.LastToken);
                if(tokenPrecedence < nextPrecedence)
                {
                    RHS = ParseBinOpRHS(tokenPrecedence + 1, RHS);
                }

                LHS = new BinaryExpressionAST(LHS, RHS, _operator);
            }
        }

        private int GetPrecedence(Token token)
        {
            if (!BinopPrecedence.ContainsKey(token.Value[0].ToString())) return -1;
            return BinopPrecedence[token.Value[0].ToString()];
        }
    }
}
