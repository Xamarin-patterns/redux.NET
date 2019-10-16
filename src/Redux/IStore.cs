using System;

namespace Redux
{
    public interface IStore<TState> : IObservable<StateChangedArgs<TState>>
    {
        IAction Dispatch(IAction action , IProgress<int> progress = null);

        TState GetState();
    }
}