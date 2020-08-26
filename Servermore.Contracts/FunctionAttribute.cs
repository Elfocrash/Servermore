using System;

namespace Servermore.Contracts
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionAttribute : Attribute
    {
        public string Name { get; }

        public FunctionAttribute(string functionName)
        {
            Name = functionName;
        }
    }
}
