using System;

namespace Redux
{
    public interface IStore<TState> : IObservable<TState>
    {
        IAction Dispatch(IAction action , IProgress<int> progress = null);

        TState GetState();
    }
}