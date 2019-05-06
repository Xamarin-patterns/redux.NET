using System;

namespace Redux
{
    public delegate TState Reducer<TState>(TState previousState, IAction action , IProgress<int> progress);
}