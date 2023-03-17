using SmolScript.Internals.Ast.Statements;

namespace SmolScript.Internals.Ast.Interpreter
{
    /// <summary>
    /// Used by the AST interpreter to package everything needed to let us
    /// call this function at some point in the future
    /// </summary>
    public class SmolFunctionWrapper : ICallableSmolFunction
    {
        public FunctionStatement declaration { get; private set; }
        private readonly Environment closure;

        public SmolFunctionWrapper(FunctionStatement declaration, Environment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }

        public object? call(AstInterpreter interpreter, IList<object?> parameters)
        {
            var env = new Environment(this.closure);

            for(int i = 0; i < declaration.parameters.Count(); i++)
            {
                if (parameters.Count() > i)
                {
                    var anonymousFunction = parameters[i] as FunctionStatement;

                    if (anonymousFunction != null)
                    {
                        env.Define(declaration.parameters[i].lexeme, new SmolFunctionWrapper((FunctionStatement)anonymousFunction, env));
                    }
                    else
                    {
                        env.Define(declaration.parameters[i].lexeme, parameters[i]);
                    }
                }
                else
                {
                    env.Define(declaration.parameters[i].lexeme, null);
                }
            }

            object? returnValue = null;

            try
            {
                interpreter.executeBlock(declaration.functionBody.statements, env);
            }
            catch (ReturnFromUserDefinedFunction r)
            {
                returnValue = r.ReturnValue;
            }

            return returnValue;
        }
    }
}