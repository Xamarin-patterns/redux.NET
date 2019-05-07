using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Redux
{
    public class AsyncStore<TState> : Store<Task<TState>>
    {
        private readonly ILogger _logger;

        public AsyncStore(Reducer<Task<TState>> reducer, Task<TState> initialState = null)
            : base(reducer, initialState)
        {
        }
        public AsyncStore(Reducer<Task<TState>> reducer, ILogger logger, Task<TState> initialState = null)
            : base(reducer, initialState)
        {
            _logger = logger;
        }
        public void LogError(Exception ex)
        {
            if(_logger?.IsEnabled(LogLevel.Error)==true)
             _logger.LogError(ex,ex.Message);
        }

        public void LogInformation(IAction action)
        {
            if (_logger?.IsEnabled(LogLevel.Information)==true)
                _logger.LogInformation($"{action} excuted at {DateTime.Now}");
        }

        public Task DispatchAsync(IAction action, IProgress<int> progress = null)
        {
            var lastState = _reducer.Invoke(_lastState, action, progress);
            lastState.ContinueWith(task =>
            {
                _lastState = task;
                LogInformation(action);
                _stateSubject.OnNext(_lastState);

            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            lastState.ContinueWith(task =>
            {
                LogError(task.Exception);
                _faultedSubject.OnNext(new ActionDispatchingException(action, task.Exception));
            }, TaskContinuationOptions.NotOnRanToCompletion);
            return lastState;
        }
    }
}