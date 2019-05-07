using System;

namespace Redux
{
    public class  ActionDispatchingException:Exception
    {
        public ActionDispatchingException(IAction action, Exception exception):
            base($"Error while dispatching action {action}",exception)
        {
            Action = action;
        }

        public IAction Action { get; }

    }
}