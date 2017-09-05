using System;
using Karambolo.Common.Properties;

namespace Karambolo.Common
{
    public interface IErrorStateReporter<out TState>
    {
        bool ThrowOnError { get; set; }
        TState Error { get; }
    }

    public class ErrorStateReporter<TState>
    {
        protected void HandleError(TState error, string messageFormat, Func<string, Exception> exceptionFactory, params object[] messageArgs)
        {
            if (ThrowOnError)
            {
                var ex = exceptionFactory(
                    messageArgs != null && messageArgs.Length > 0 ?
                    string.Format(messageFormat, messageArgs) :
                    messageFormat);
                ex.Data["ErrorState"] = error;
                throw ex;
            }
            else
                Error = error;
        }

        protected virtual bool ThrowOnErrorCanChanged()
        {
            return true;
        }

        bool _throwOnError = true;
        public bool ThrowOnError
        {
            get => _throwOnError;
            set
            {
                if (!ThrowOnErrorCanChanged())
                    throw new InvalidOperationException(Resources.ErrorHandlingCannotSet);

                _throwOnError = value;
            }
        }

        public TState Error { get; protected set; }
    }
}
