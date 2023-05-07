using System;
namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolFunction : SmolVariableType
    {
        internal string? global_function_name;
        internal int code_section;
        internal int arity;
        internal List<string> param_variable_names = new List<string>();

        internal SmolFunction(string? global_function_name, int code_section, int arity, List<string> param_variable_names)
        {
            this.global_function_name = global_function_name;
            this.code_section = code_section;
            this.arity = arity;
            this.param_variable_names = param_variable_names;
        }

        internal override object? GetValue()
        {
            return this;
        }

        public override string ToString()
        {
            return "[SmolFunction]";
        }
    }
}

