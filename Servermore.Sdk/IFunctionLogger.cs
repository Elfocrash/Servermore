using Microsoft.Extensions.Logging;

namespace Servermore.Sdk
{
    public interface IFunctionLogger
    {
        void Log(string message);
    }

    public class FunctionLogger : IFunctionLogger
    {
        private readonly ILogger<FunctionLogger> _logger;

        public FunctionLogger(ILogger<FunctionLogger> logger)
        {
            _logger = logger;
        }

        public void Log(string message)
        {
            _logger.LogInformation(message);
        }
    }
}
