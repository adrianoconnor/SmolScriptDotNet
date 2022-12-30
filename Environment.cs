namespace SmolScript
{
    public class Environment
    {
        private readonly Environment? enclosing = null;
        private IDictionary<string, object?> variables = new Dictionary<string, object?>();

        public Environment()
        {

        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
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

    }
}