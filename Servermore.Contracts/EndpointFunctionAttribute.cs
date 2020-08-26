namespace Servermore.Contracts
{
    public class EndpointFunctionAttribute : FunctionAttribute
    {
        public string Route { get; }

        public EndpointFunctionAttribute(string functionName, string route) : base(functionName)
        {
            Route = route;
        }
    }
}
