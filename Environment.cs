namespace SmolScript
{
    public class Environment
    {
        public enum Scope { Global, Function, Block };

        public Scope? scope;

        private readonly Environment? enclosing = null;
        private IDictionary<string, object?> variables = new Dictionary<string, object?>();
        public object? returnVaue = null;

        public Environment()
        {
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public Environment(Environment enclosing, Scope scope)
        {
            this.enclosing = enclosing;
            this.scope = scope;
        }

        public void Define(string variable, object? value)
        {
            variables[variable] = value;
        }

        public void Assign(string variable, object? value)
        {
            if (variables.ContainsKey(variable))
            {
                variables[variable] = value;
            }
            else if (enclosing != null)
            {
                enclosing.Assign(variable, value);
            }
            else
            {
                throw new Exception("Variable undefined");
            }
        }

        public object? Get(string variable)
        {
            if (variables.ContainsKey(variable))
            {
                return variables[variable];
            }
            else if (enclosing != null)
            {
                return enclosing.Get(variable);
            }
            else
            {
                throw new Exception("Variable undefined");
            }
        }

        public void SetFunctionReturnValue(object? value)
        {
            if (this.scope.HasValue && this.scope == Scope.Function)
            {
                this.returnVaue = value;
            }
            else if (this.enclosing != null) 
            {
                this.enclosing.SetFunctionReturnValue(value);
            }
            else
            {
                throw new RuntimeError("Unable to locate variable scope for function");
            }
        }

    }
}