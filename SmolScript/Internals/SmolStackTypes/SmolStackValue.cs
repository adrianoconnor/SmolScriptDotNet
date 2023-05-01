using System.Diagnostics.CodeAnalysis;

namespace SmolScript.Internals.SmolStackTypes
{
    /// <summary>
    /// This class represents any kind of runtime variable that can be put on
    /// the stack. I'm not sure it's the best name, because it can also hold
    /// function definitions, class instances etc. Also, there's nothing in
    /// the VM right now that's a .net value type -- everything gets boxed.
    /// </summary>
    abstract internal class SmolStackValue
    {
        abstract internal object? GetValue();

        internal string GetTypeName()
        {
            var type = this.GetType().ToString();

            return type.Substring(type.LastIndexOf(".") + 1).Replace("Smol", "");
        }

        internal static SmolStackValue Create(object? value)
        {
            if (value == null)
            {
                return new SmolNull();
            }
            else
            {
                var t = value.GetType();

                if (t == typeof(string))
                {
                    return new SmolString((string)value);
                }
                else if (t == typeof(double))
                {
                    return new SmolNumber((double)value);
                }
                else if (t == typeof(int))
                {
                    return new SmolNumber(Convert.ToDouble((int)value));
                }
                else if (t == typeof(Int64))
                {
                    return new SmolNumber(Convert.ToDouble((Int64)value));
                }
                else if (t == typeof(bool))
                {
                    return new SmolBool((bool)value);
                }
                else
                {
                    throw new Exception($"Could not create a valid stack value from: {value.GetType()}");
                }
            }
        }
        /*
        public override string ToString()
        {
            return $"({this.type.ToString()}) {this.value}";
        }
        */
        public static SmolStackValue operator +(SmolStackValue a, SmolStackValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = ((SmolNumber)a).value;
                var right = ((SmolNumber)b).value;

                return new SmolNumber(left + right);
            }
            else if(a.GetType() == typeof(SmolString) || b.GetType() == typeof(SmolString))
            {
                // TODO: Need a Stringify helper method.

                string aString = a.GetValue()!.ToString();
                string bString = b.GetValue()!.ToString();

                return new SmolString(aString + bString);
            }           

            throw new Exception($"Unable to add {a.GetType()} and {b.GetType()}");
        }
        
        public static SmolStackValue operator -(SmolStackValue a, SmolStackValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolStackValue.Create(left - right);
            }

            throw new Exception($"Unable to subtract {a.GetType()} and {b.GetType()}");
        }

        public static SmolStackValue operator *(SmolStackValue a, SmolStackValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolStackValue.Create(left * right);
            }

            throw new Exception($"Unable to multiply {a.GetTypeName()} and {b.GetTypeName()}");
        }

        public static SmolStackValue operator /(SmolStackValue a, SmolStackValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolStackValue.Create(left / right);
            }

            throw new Exception($"Unable to divide {a.GetType()} and {b.GetType()}");
        }

        public static SmolStackValue operator %(SmolStackValue a, SmolStackValue b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolStackValue.Create(left % right);
            }

            throw new Exception($"Unable to modulo {a.GetType()} and {b.GetType()}");
        }

        public static SmolStackValue operator >(SmolStackValue a, SmolStackValue b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolStackValue.Create(left > right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolStackValue operator <(SmolStackValue a, SmolStackValue b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolStackValue.Create(left < right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolStackValue operator >=(SmolStackValue a, SmolStackValue b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolStackValue.Create(left >= right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolStackValue operator <=(SmolStackValue a, SmolStackValue b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolStackValue.Create(left <= right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolStackValue operator |(SmolStackValue a, SmolStackValue b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = Convert.ToInt64((double)a.GetValue()!);
                var right = Convert.ToInt64((double)b.GetValue()!);

                return SmolStackValue.Create(left | right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolStackValue operator &(SmolStackValue a, SmolStackValue b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = Convert.ToInt64((double)a.GetValue()!);
                var right = Convert.ToInt64((double)b.GetValue()!);

                return SmolStackValue.Create(left & right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public SmolStackValue Power(SmolStackValue power)
        {                
            if (this.GetType() == typeof(SmolNumber) && power.GetType() == typeof(SmolNumber))
            {
                var left = (double)((SmolNumber)this).GetValue()!;
                var right = (double)((SmolNumber)power).GetValue()!;

                return SmolStackValue.Create(double.Pow(left, right));
            }

            throw new Exception($"Unable to calculate power for");// {this.type} and {power.type}");
        }
                
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (this.GetType() == typeof(SmolNumber) && obj?.GetType() == typeof(SmolNumber))
            {
                return ((SmolNumber)this).value.Equals(((SmolNumber)obj!).value);
            }
            if (this.GetType() == typeof(SmolString))
            {
                return ((SmolString)this).value == ((SmolString)obj!).value;
            }
            else
            {
                return base.Equals(obj);
            }            
        }
        
        public override int GetHashCode()
        {
            if (this.GetType() == typeof(SmolNumber))
            {
                return ((SmolNumber)this).value.GetHashCode();
            }
            if (this.GetType() == typeof(SmolString))
            {
                return ((SmolString)this).value.GetHashCode();
            }
            else 
            {
                return base.GetHashCode();
            }
        }

        public bool IsTruthy()
        {
            return (bool)this.GetValue()! == true;
        }

        public bool IsFalsey()
        {
            return !IsTruthy();
        }
    }
}

