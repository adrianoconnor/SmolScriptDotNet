using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolArray : SmolVariableType, ISmolNativeCallable
    {
        internal readonly List<SmolVariableType> Elements = new List<SmolVariableType>();

        internal SmolArray()
        {
        }

        internal SmolArray(List<SmolVariableType> elements)
        {
            this.Elements = elements;
        }


        internal override object? GetValue()
        {
            return null;
        }

        public override string ToString()
        {
            return $"(SmolArray) length={this.Elements.Count}";
        }

        public SmolVariableType GetProp(string propName)
        {
            switch (propName)
            {
                case "length":
                    return new SmolNumber(this.Elements.Count);

                default:

                    if (int.TryParse(propName, out int index))
                    {
                        var result = this.Elements[index];

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

                while (index >= this.Elements.Count())
                {
                    Elements.Add(new SmolUndefined());
                }

                this.Elements[index] = value;
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
                    var el = this.Elements.Last();
                    this.Elements.RemoveAt(this.Elements.Count() - 1);
                    return el;

                case "push":
                    this.Elements.Add(parameters[0]);
                    return parameters[0];

                case "slice":
                {
                    var first = 0;
                    var last = this.Elements.Count;

                    if (parameters.Count > 0)
                    {
                        var p1 = (int)Math.Truncate(((SmolNumber)parameters[0]).NumberValue);

                        if (p1 < 0)
                        {
                            first = this.Elements.Count - Math.Abs(p1);
                        }
                        else
                        {
                            first = p1;
                        }

                        p1 = Math.Min(p1, this.Elements.Count - 1);
                    }

                    if (parameters.Count > 1)
                    {
                        var p2 = (int)Math.Truncate(((SmolNumber)parameters[1]).NumberValue);

                        if (p2 < 0)
                        {
                            last = this.Elements.Count - Math.Abs(p2);
                        }
                        else
                        {
                            last = p2;
                        }
                    }
                    
                    // TODO: Clamp within bounds of array and also handle null etc.
                    
                    return new SmolArray(this.Elements.Slice(first, last - first));
                }

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
                        array.Elements.Add(p);
                    }

                    return array;

                default:
                    throw new Exception($"String class cannot handle static function {funcName}");
            }
        }
    }
}
