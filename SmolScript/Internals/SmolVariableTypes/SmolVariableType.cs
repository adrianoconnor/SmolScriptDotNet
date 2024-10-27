using System;
using System.Diagnostics.CodeAnalysis;
using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals.SmolVariableTypes
{
    internal abstract class SmolVariableType : SmolStackType
    {
        internal abstract object? GetValue();

        internal string GetTypeName()
        {
            var type = this.GetType().ToString();

            return type.Substring(type.LastIndexOf(".") + 1).Replace("Smol", "");
        }

        internal static SmolVariableType Create(object? value)
        {
            if (value == null)
            {
                return new SmolNull();
            }
            else
            {
                var tryCastValue = value as SmolVariableType;

                if (tryCastValue != null)
                {
                    return tryCastValue;
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

                    throw new Exception($"Could not create a valid SmolVariable object from {value.GetType()}");
                }
            }
        }

        public static SmolVariableType operator +(SmolVariableType a, SmolVariableType b)
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
            else if (a.GetType() == typeof(SmolString) || b.GetType() == typeof(SmolString))
            {
                // TODO: Need a Stringify helper method.

                var aString = a.GetValue()!.ToString()!;
                var bString = b.GetValue()!.ToString()!;

                return new SmolString(aString + bString);
            }

            throw new Exception($"Unable to add {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator -(SmolVariableType a, SmolVariableType b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolVariableType.Create(left - right);
            }

            throw new Exception($"Unable to subtract {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator *(SmolVariableType a, SmolVariableType b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolVariableType.Create(left * right);
            }

            throw new Exception($"Unable to multiply {a.GetTypeName()} and {b.GetTypeName()}");
        }

        public static SmolVariableType operator /(SmolVariableType a, SmolVariableType b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolVariableType.Create(left / right);
            }

            throw new Exception($"Unable to divide {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator %(SmolVariableType a, SmolVariableType b)
        {
            // Currently only handles really simple case of both values
            // being numeric -- anything else will raise an exception. Next
            // to implement is string handling...

            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolVariableType.Create(left % right);
            }

            throw new Exception($"Unable to modulo {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator >(SmolVariableType a, SmolVariableType b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolVariableType.Create(left > right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator <(SmolVariableType a, SmolVariableType b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolVariableType.Create(left < right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator >=(SmolVariableType a, SmolVariableType b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolVariableType.Create(left >= right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator <=(SmolVariableType a, SmolVariableType b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = (double)a.GetValue()!;
                var right = (double)b.GetValue()!;

                return SmolVariableType.Create(left <= right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator |(SmolVariableType a, SmolVariableType b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = Convert.ToInt64((double)a.GetValue()!);
                var right = Convert.ToInt64((double)b.GetValue()!);

                return SmolVariableType.Create(left | right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public static SmolVariableType operator &(SmolVariableType a, SmolVariableType b)
        {
            if (a.GetType() == typeof(SmolNumber) && b.GetType() == typeof(SmolNumber))
            {
                var left = Convert.ToInt64((double)a.GetValue()!);
                var right = Convert.ToInt64((double)b.GetValue()!);

                return SmolVariableType.Create(left & right);
            }

            throw new Exception($"Unable to compare {a.GetType()} and {b.GetType()}");
        }

        public SmolVariableType Power(SmolVariableType power)
        {
            if (this.GetType() == typeof(SmolNumber) && power.GetType() == typeof(SmolNumber))
            {
                var left = (double)((SmolNumber)this).GetValue()!;
                var right = (double)((SmolNumber)power).GetValue()!;

                return SmolVariableType.Create(Math.Pow(left, right));
            }

            throw new Exception($"Unable to calculate power for");// {this.type} and {power.type}");
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (this.GetType() == typeof(SmolNumber) && obj?.GetType() == typeof(SmolNumber))
            {
                return ((SmolNumber)this).value.Equals(((SmolNumber)obj!).value);
            }
            else if (this.GetType() == typeof(SmolString))
            {
                return ((SmolString)this).value == ((SmolString)obj!).value;
            }
            else if (this.GetType() == typeof(SmolUndefined))
            {
                return obj?.GetType() == typeof(SmolUndefined);
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

