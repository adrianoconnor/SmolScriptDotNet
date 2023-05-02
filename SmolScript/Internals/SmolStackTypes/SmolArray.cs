using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmolScript.Internals.SmolStackTypes
{
	internal class SmolArray: SmolStackValue, ISmolNativeCallable
	{
        internal readonly List<SmolStackValue> elements = new List<SmolStackValue>();

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

        public SmolStackValue GetProp(string propName)
        {
            switch (propName)
            {
                case "length":
                    return new SmolNumber(this.elements.Count);

                default:
                    throw new Exception($"{this.GetTypeName()} does not contain property {propName}");
            }
        }

        public SmolStackValue NativeCall(string funcName, List<SmolStackValue> parameters)
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

        public static SmolStackValue StaticCall(string funcName, List<SmolStackValue> parameters)
        { 
            switch (funcName)
            {
                case "constructor":

                    var array = new SmolArray();

                    foreach(var p in parameters)
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
