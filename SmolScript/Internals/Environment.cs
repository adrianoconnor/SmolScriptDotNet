using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals
{
    internal class Environment
    {
        public readonly Environment? Enclosing = null;
        internal IDictionary<string, SmolVariableType> Variables = new Dictionary<string, SmolVariableType>();

        public Environment()
        {
        }

        public Environment(Environment enclosing)
        {
            this.Enclosing = enclosing;
        }

        public void Define(string variable, SmolVariableType value)
        {
            Variables[variable] = value;
        }

        public void Assign(string variable, SmolVariableType value, bool isThis = false)
        {
            if (Variables.ContainsKey(variable))
            {
                Variables[variable] = value;
            }
            else if (isThis)
            {
                // Having to do this for now so that class ivars work
                this.Define(variable, value);
            }
            else if (Enclosing != null)
            {
                Enclosing.Assign(variable, value);
            }
            else
            {
                throw new SmolRuntimeException("Variable undefined");
            }
        }

        public object? Get(string variable)
        {
            if (Variables.ContainsKey(variable))
            {
                return Variables[variable];
            }
            else if (Enclosing != null)
            {
                return Enclosing.Get(variable);
            }
            else
            {
                throw new SmolRuntimeException("Variable undefined");
            }
        }

        public object? TryGet(string variable)
        {
            if (Variables.ContainsKey(variable))
            {
                return Variables[variable];
            }
            else if (Enclosing != null)
            {
                return Enclosing.TryGet(variable);
            }
            else
            {
                return null;
            }
        }
    }
}