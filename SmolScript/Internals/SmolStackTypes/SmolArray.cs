using System;

namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolArray : SmolVariableType, ISmolNativeCallable
    {
        internal readonly List<SmolVariableType> elements = new List<SmolVariableType>();

        internal SmolArray()
        {
        }

        internal override object? GetValue()
        {
            return null;
        }

        public override string ToString()
        {
            return $"(SmolArray) length={this.elements.Count}";
        }

        public SmolVariableType GetProp(string propName)
        {
            switch (propName)
            {
                case "length":
                    return new SmolNumber(this.elements.Count);

                default:

                    if (int.TryParse(propName, out int index))
                    {
                        return this.elements[index];
                    }

                    throw new Exception($"{this.GetTypeName()} does not contain property {propName}");
            }
        }

        public void SetProp(string propName, SmolVariableType value)
        {
            if (int.TryParse(propName, out int index))
            {
                while (index > this.elements.Count() - 1)
                {
                    elements.Add(new SmolUndefined());
                }

                this.elements[index] = value;
            }
            else
            {
                throw new Exception($"Not a valid index");
            }
        }

        public SmolVariableType NativeCall(string funcName, List<SmolVariableType> parameters)
        {
            switch (funcName)
            {
                case "pop":
                    var el = this.elements.Last();
                    this.elements.RemoveAt(this.elements.Count() - 1);
                    return el;

                case "push":
                    this.elements.Add(parameters[0]);
                    return parameters[0];

                default:
                    throw new Exception($"{this.GetTypeName()} cannot handle native function {funcName}");
            }
        }

        public static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters)
        {
            switch (funcName)
            {
                case "constructor":

                    var array = new SmolArray();

                    foreach (var p in parameters)
                    {
                        array.elements.Add(p);
                    }

                    return array;

                default:
                    throw new Exception($"String class cannot handle static function {funcName}");
            }
        }
    }
}
