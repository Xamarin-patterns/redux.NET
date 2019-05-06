using System;

namespace Redux
{
    public delegate Func<Dispatcher, Dispatcher> Middleware<TState>(IStore<TState> store);
}