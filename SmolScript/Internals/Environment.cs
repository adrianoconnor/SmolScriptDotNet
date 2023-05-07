using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals
{
    internal class Environment
    {
        public readonly Environment? enclosing = null;
        private IDictionary<string, SmolVariableType> variables = new Dictionary<string, SmolVariableType>();

        public Environment()
        {
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string variable, SmolVariableType value)
        {
            variables[variable] = value;
        }

        public void Assign(string variable, SmolVariableType value, bool isThis = false)
        {
            if (variables.ContainsKey(variable))
            {
                variables[variable] = value;
            }
            else if (isThis)
            {
                // Having to do this for now so that class ivars work
                this.Define(variable, value);
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

        public object? TryGet(string variable)
        {
            if (variables.ContainsKey(variable))
            {
                return variables[variable];
            }
            else if (enclosing != null)
            {
                return enclosing.TryGet(variable);
            }
            else
            {
                return null;
            }
        }
    }
}