using System;

namespace TextEditor
{
    public static class TokenCodes
    {
        public const int WhileKeyword = 101;
        public const int DoKeyword = 102;
        public const int ForKeyword = 103;
        public const int IfKeyword = 104;
        public const int ThenKeyword = 105;
        public const int ElseKeyword = 106;
        public const int LetKeyword = 107;
        public const int InKeyword = 108;
        public const int MatchKeyword = 109;
        public const int WithKeyword = 110;
        public const int FunctionKeyword = 111;
        public const int FunKeyword = 112;
        public const int RecKeyword = 113;
        public const int ModuleKeyword = 114;
        public const int OpenKeyword = 115;
        public const int TypeKeyword = 116;
        public const int ID = 117;
        public const int Num = 202;
        public const int Float = 203;
        public const int Less = 301;
        public const int Greater = 302;
        public const int LessOrEqual = 303;
        public const int GreaterOrEqual = 304;
        public const int Equal = 305;
        public const int NotEqual = 306;
        public const int Add = 307;
        public const int Sub = 308;
        public const int Mul = 309;
        public const int Div = 310;
        public const int Mod = 311;
        public const int And = 312;
        public const int Or = 313;
        public const int Not = 314;
        public const int Cons = 315;
        public const int Arrow = 316;
        public const int Assign = 317;
        public const int Dot = 318;
        public const int Semicolon = 401;
        public const int Comma = 402;
        public const int Colon = 403;
        public const int LeftParen = 501;
        public const int RightParen = 502;
        public const int LeftBracket = 503;
        public const int RightBracket = 504;
        public const int LeftBrace = 505;
        public const int RightBrace = 506;
        public const int VerticalBar = 507;
        public const int Error = -1;
    }
}