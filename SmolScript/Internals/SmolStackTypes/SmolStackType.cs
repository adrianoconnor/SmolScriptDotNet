using System.Diagnostics.CodeAnalysis;

namespace SmolScript.Internals.SmolStackTypes
{
    /// <summary>
    /// This class represents any kind of runtime variable that can be put on
    /// the stack. I'm not sure it's the best name, because it can also hold
    /// function definitions, class instances etc. Also, there's nothing in
    /// the VM right now that's a .net value type -- everything gets boxed.
    /// </summary>
    internal abstract class SmolStackType
    {
    }
}

