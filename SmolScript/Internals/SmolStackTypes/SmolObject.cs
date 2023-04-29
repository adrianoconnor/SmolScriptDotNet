using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolObject : SmolStackValue
    {
        internal Environment object_env;
        internal string class_name;

        internal SmolObject(Environment object_env, string class_name)
		{
            this.object_env = object_env;
            this.class_name = class_name;
        }

        internal override object? GetValue()
        {
            return null;
        }
    }
}

