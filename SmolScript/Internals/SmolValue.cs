namespace SmolScript.Internals
{
    /// <summary>
    /// This class represents any kind of runtime variable that can be put on
    /// the stack. I'm not sure it's the best name, because it can also hold
    /// function definitions, class instances etc. Also, there's nothing in
    /// the VM right now that's a .net value type -- everything gets boxed.
    /// </summary>
    public struct SmolValue
    {
        public SmolValueType type { get; set; }
        public object? value { get; set; }

        public SmolValue(object? value)
        {
            if (value == null)
            {
                type = SmolValueType.Null;
            }
            else
            {
                var t = value.GetType();

                if (t == typeof(string))
                {
                    this.type = SmolValueType.String;
                    this.value = (string)value;
                }
                else if (t == typeof(double))
                {
                    this.type = SmolValueType.Number;
                    this.value = (double)value;
                }
                else if (t == typeof(int))
                {
                    this.type = SmolValueType.Number;
                    this.value = (int)value;
                }
                else if (t == typeof(bool))
                {
                    this.type = SmolValueType.Bool;
                    this.value = (bool)value;
                }
                else
                {
                    this.type = SmolValueType.Unknown;
                    this.value = value;
                }
            }
        }

        public override string ToString()
        {
            return $"({this.type.ToString()}) {this.value}";
        }
   
        public static SmolValue operator +(SmolValue a, SmolValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            var right = (double)a.value!;
            var left = (double)b.value!;

            return new SmolValue()
            {
                type = SmolValueType.Number,
                value = left + right
            };
        }

        public bool IsFalsey()
        {
            return (bool)this.value! == false;
        }
    }
}

