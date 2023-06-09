﻿using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals.SmolVariableTypes
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
                        var result = this.elements[index];

                        return result ?? new SmolUndefined();
                    }

                    throw new Exception($"{this.GetTypeName()} does not contain property {propName}");
            }
        }

        public void SetProp(string propName, SmolVariableType value)
        {
            if (int.TryParse(propName, out int index))
            {
                // If we have an array with 2 elements ([0] and [1]), and we want to set 
                // The element at index [3], we need to insert undefined at [2]

                while (index >= this.elements.Count())
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
