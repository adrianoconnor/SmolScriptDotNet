using System.Diagnostics.CodeAnalysis;

namespace SmolScript.Internals
{
    /// <summary>
    /// This class represents any kind of runtime variable that can be put on
    /// the stack. I'm not sure it's the best name, because it can also hold
    /// function definitions, class instances etc. Also, there's nothing in
    /// the VM right now that's a .net value type -- everything gets boxed.
    /// </summary>
    internal struct SmolValue
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

            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Number,
                    value = left + right
                };
            }
            else if (a.type == SmolValueType.String || b.type == SmolValueType.String)
            {
                // TODO: Need a Stringify helper method.

                string aString = a.type == SmolValueType.String ? (string)a.value! : ((double)a.value!).ToString();
                string bString = b.type == SmolValueType.String ? (string)b.value! : ((double)b.value!).ToString();

                return new SmolValue()
                {
                    type = SmolValueType.String,
                    value = aString + bString
                };
            }

            throw new Exception($"Unable to add {a.type} and {b.type}");
        }

        public static SmolValue operator -(SmolValue a, SmolValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Number,
                    value = left - right
                };
            }

            throw new Exception($"Unable to subtract {a.type} and {b.type}");
        }

        public static SmolValue operator *(SmolValue a, SmolValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Number,
                    value = left * right
                };
            }

            throw new Exception($"Unable to multiply {a.type} and {b.type}");
        }

        public static SmolValue operator /(SmolValue a, SmolValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Number,
                    value = left / right
                };
            }

            throw new Exception($"Unable to divide {a.type} and {b.type}");
        }

        public static SmolValue operator %(SmolValue a, SmolValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Number,
                    value = left % right
                };
            }

            throw new Exception($"Unable to modulo {a.type} and {b.type}");
        }

        public static SmolValue operator >(SmolValue a, SmolValue b)
        {
            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Bool,
                    value = left > right
                };
            }

            throw new Exception($"Unable to compare {a.type} and {b.type}");
        }

        public static SmolValue operator <(SmolValue a, SmolValue b)
        {
            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Bool,
                    value = left < right
                };
            }

            throw new Exception($"Unable to compare {a.type} and {b.type}");
        }

        public static SmolValue operator >=(SmolValue a, SmolValue b)
        {
            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Bool,
                    value = left >= right
                };
            }

            throw new Exception($"Unable to compare {a.type} and {b.type}");
        }

        public static SmolValue operator <=(SmolValue a, SmolValue b)
        {
            if (a.type == SmolValueType.Number && b.type == SmolValueType.Number)
            {
                var left = (double)a.value!;
                var right = (double)b.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Bool,
                    value = left <= right
                };
            }

            throw new Exception($"Unable to compare {a.type} and {b.type}");
        }

        public SmolValue Power(SmolValue power)
        {
            if (this.type == SmolValueType.Number && power.type == SmolValueType.Number)
            {
                var left = (double)this.value!;
                var right = (double)power.value!;

                return new SmolValue()
                {
                    type = SmolValueType.Number,
                    value = double.Pow(left, right)
                };
            }

            throw new Exception($"Unable to calculate power for {this.type} and {power.type}");
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (this.type == SmolValueType.Number)
            {
                return ((double)this.value!).GetHashCode();
            }
            if (this.type == SmolValueType.String)
            {
                return ((string)this.value!).GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public bool IsTruthy()
        {
            return (bool)this.value! == true;
        }

        public bool IsFalsey()
        {
            return !IsTruthy();
        }
    }
}

