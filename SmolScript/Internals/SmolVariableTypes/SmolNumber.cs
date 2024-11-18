namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolNumber : SmolVariableType, ISmolNativeCallable
    {
        internal readonly double NumberValue;

        internal SmolNumber(double numberValue)
        {
            this.NumberValue = numberValue;
        }

        internal override object? GetValue()
        {
            return this.NumberValue;
        }

        public override string ToString()
        {
            return $"(Number) {NumberValue}";
        }

        public override int GetHashCode()
        {
            return this.NumberValue.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (this.GetType() == typeof(SmolNumber) && obj?.GetType() == typeof(SmolNumber))
            {
                return ((SmolNumber)this).NumberValue.Equals(((SmolNumber)obj!).NumberValue);
            }
            else
            {
                return base.Equals(obj);
            }
        }
        
        public SmolVariableType GetProp(string propName)
        {
            throw new Exception($"{this.GetType()} cannot handle native property {propName}");
        }

        public void SetProp(string propName, SmolVariableType value)
        {
            throw new Exception($"Not a valid target");
        }

        public SmolVariableType NativeCall(string funcName, List<SmolVariableType> parameters)
        {
            switch (funcName)
            {
                case "toString": 
                    return new SmolString(this.NumberValue.ToString());
                default:
                    throw new Exception($"{this.GetType()} cannot handle native function {funcName}");
            }
        }

        public static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters)
        {
            throw new Exception($"Number class cannot handle static function {funcName}");
        }
    }
}

