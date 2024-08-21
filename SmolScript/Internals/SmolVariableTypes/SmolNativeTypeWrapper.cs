using System.Reflection;

namespace SmolScript.Internals.SmolVariableTypes
{
    /// <summary>
    /// This class wraps a .net object that is passed in as a parameter. If the code tries to call
    /// a method or access a value, we use reflection to call it dynamically at runtime.
    /// </summary>
    internal class SmolNativeTypeWrapper : SmolVariableType, ISmolNativeCallable
    {
        internal readonly object obj;

        internal SmolNativeTypeWrapper(object obj)
        {
            this.obj = obj;
        }

        internal override object? GetValue()
        {
            return this.obj;
        }

        public override string ToString()
        {
            return $"(SmolNativeTypeWrapper) {obj}";
        }

        public SmolVariableType GetProp(string propName)
        {
            try
            {
                return SmolVariableType.Create(obj.GetType().GetFields().First(f => f.Name == propName).GetValue(obj));
            }
            catch { }

            try
            {
                return SmolVariableType.Create(obj.GetType().GetProperties(BindingFlags.Public| BindingFlags.Instance).First(f => f.Name == propName).GetValue(obj));
            }
            catch { }


            throw new Exception($"There is no valid public field or getter property on {obj.GetType().Name} called {propName}");
        }

        public void SetProp(string propName, SmolVariableType value)
        {
            try
            {
                var targetType = obj.GetType().GetFields().FirstOrDefault(f => f.Name == propName)?.FieldType;

                if (targetType != null)
                {
                    var coercedValue = Convert.ChangeType(value.GetValue(), targetType);

                    obj.GetType().GetFields().First(f => f.Name == propName).SetValue(obj, coercedValue);
                    return;
                }
            }
            catch { }

            try
            {
                var targetType = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).First(f => f.Name == propName).PropertyType;
                var coercedValue = Convert.ChangeType(value.GetValue(), targetType);
                
                obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).First(f => f.Name == propName).SetValue(obj, coercedValue);
                return;
            }
            catch { }

            throw new Exception($"There is no valid public field or setter property on {obj.GetType().Name} called {propName}");
        }

        public SmolVariableType NativeCall(string funcName, List<SmolVariableType> parameters)
        {
            var method = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).First(f => f.Name == funcName);

            var returnType = method.ReturnType.Name;
            var argTypes = method.GetParameters();
            var args = new List<object>();
            var numberOfParams = argTypes.Count();
            var numberOfPassedArgs = parameters.Count();

            if (numberOfParams != numberOfPassedArgs)
            {
                throw new Exception($"{funcName} expects {numberOfParams} args, but got {numberOfPassedArgs}");
            }

            for (int i = 0; i < numberOfPassedArgs; i++)
            {
                var argInfo = argTypes[i];
                var value = parameters[i];

                if (argInfo.ParameterType.Name == "String")
                {
                    args.Add(Convert.ChangeType(value!.GetValue()!, typeof(string)));
                }
                else if (argInfo.ParameterType.Name == "Double")
                {
                    args.Add(Convert.ChangeType(value!.GetValue()!, typeof(double)));
                }
                else if (argInfo.ParameterType.Name == "Int32" || argInfo.ParameterType.Name == "Int64")
                {
                    args.Add(Convert.ChangeType(value!.GetValue()!, typeof(int)));
                }
                else if (argInfo.ParameterType.Name == "Boolean")
                {
                    args.Add(Convert.ChangeType(value!.GetValue()!, typeof(bool)));
                }
                else
                {
                    throw new Exception($"Failed to process argument {i + 1} when calling {funcName} (type {argInfo.Name})");
                }
            }

            var result = method.Invoke(obj, args.ToArray());

            if (returnType == "Void")
            {
                return new SmolUndefined();
            }
            else
            {
                return Create(result);
            }
        }

        public static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters)
        {
            switch (funcName)
            {
                default:
                    throw new Exception($"Native type cannot handle static function {funcName}");
            }
        }
    }
}
