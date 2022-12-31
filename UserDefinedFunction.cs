namespace SmolScript
{
    public class UserDefinedFunction : ICallable
    {
        public Statement.Function declaration { get; private set; }
        private readonly Environment closure;

        public UserDefinedFunction(Statement.Function declaration, Environment closure)
        {
            this.declaration = declaration;
            this.closure = closure;
        }

        public object? call(Interpreter interpreter, IList<object?> parameters)
        {
            var env = new Environment(this.closure);

            for(int i = 0; i < declaration.parameters.Count(); i++)
            {
                if (parameters.Count() > i)
                {
                    var anonymousFunction = parameters[i] as Statement.Function;

                    if (anonymousFunction != null)
                    {
                        env.Define(declaration.parameters[i].lexeme, new UserDefinedFunction((Statement.Function)anonymousFunction, env));
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