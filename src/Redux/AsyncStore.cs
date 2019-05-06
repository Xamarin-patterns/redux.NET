using System;
using System.Threading.Tasks;

namespace Redux
{
    public class AsyncStore<TState> : Store<Task<TState>>
    {
        public AsyncStore(Reducer<Task<TState>> reducer, Task<TState> initialState = null)
            : base(reducer, initialState)
        {
        }
        public Task DispatchAsync(IAction action,IProgress<int> progress=null)
        {
            var lastState = _reducer.Invoke(_lastState, action,progress);
            lastState.ContinueWith(task =>
            {
                _lastState = task;
                _stateSubject.OnNext(_lastState);

            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            lastState.ContinueWith(task =>
            {
                _faultedSubject.OnNext(new Error(action,task.Exception));
            }, TaskContinuationOptions.NotOnRanToCompletion);
            return lastState;            
        }
    }
}