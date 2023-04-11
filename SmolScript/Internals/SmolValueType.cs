namespace SmolScript.Internals
{
    internal enum SmolValueType
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
        LoopMarker,
        ObjectRef, // Not sure either
        Unknown
    }
}

