namespace SmolScript.Internals
{
    public enum SmolValueType
    {
        Null,
        Undefined,
        Void,
        Bool,
        Number,
        String,
        FunctionRef, // Not sure
        SavedCallState,
        TryCheckPoint,
        Exception,
        Unknown
    }
}

