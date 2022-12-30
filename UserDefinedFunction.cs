namespace SmolScript
{
    public class UserDefinedFunction : ICallable
    {
        public Statement.Function declaration;

        public UserDefinedFunction(Statement.Function declaration)
        {
            this.declaration = declaration;
        }

        public int arity()
        {
            return declaration.parameters.Count;
        }

        public object? call(Interpreter interpreter, IList<object?> parameters)
        {
            var env = new Environment(Interpreter.globalEnv, Environment.Scope.Function);

            for(int i = 0; i < parameters.Count(); i++)
            {
                env.Define(declaration.parameters[i].lexeme, parameters[i]);
            }

            interpreter.executeBlock(declaration.functionBody.statements, env);

            return env.returnVaue;
        }
    }
}