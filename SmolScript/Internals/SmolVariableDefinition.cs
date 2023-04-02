namespace SmolScript.Internals
{
    internal struct SmolVariableDefinition
    {
        public string name { get; set; }

        public override string ToString()
        {
            return $"(var) {this.name}";
        }
    }
}

