using System;
using SmolScript.Internals;

namespace SmolScript
{
    public enum CompilerErrorSource
    {
        SCANNER,
        PARSER
    }
    
    public class SmolCompilerError : Exception
    {
        public CompilerErrorSource ErrorSource { get; set; } 
        public IList<ParseError>? ParserErrors = null;
        
        public SmolCompilerError(IList<ParseError>? errors, string message) : base(message)
        {
            this.ErrorSource = CompilerErrorSource.PARSER;
            this.ParserErrors = errors;
        }

        public SmolCompilerError(string message, Exception innerException, CompilerErrorSource source) : base(message, innerException)
        {
            ErrorSource = source;
        }

        public static SmolCompilerError ScannerError(string message)
        {
            return new SmolCompilerError(message, null, CompilerErrorSource.SCANNER);
        }
    }
    
    public class ParseError : Exception
    {
        public int LineNumber { get; set; }
        
        internal ParseError(Token token, string message) :
            base(message)
        {
            this.LineNumber = token.Line;
        }
    }
}

