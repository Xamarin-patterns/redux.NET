using System;

namespace Redux
{
    public class Error
    {
        public Error(IAction action, Exception exception)
        {
            Action = action;
            Exception = exception;
        }

        public IAction Action { get; }
        public Exception Exception { get; }

    }
}