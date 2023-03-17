namespace SmolScript.Internals
{
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

        public static SmolValue operator +(SmolValue a, SmolValue b)
        {
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

