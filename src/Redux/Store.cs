using System;
using System.Reactive.Subjects;

namespace Redux
{
    public class Store<TState> : IStore<TState>
    {
        private readonly object _syncRoot = new object();
        private readonly Dispatcher _dispatcher;
        protected readonly Reducer<TState> _reducer;
        protected readonly ReplaySubject<TState> _stateSubject = new ReplaySubject<TState>(1);
        protected readonly ReplaySubject<Error> _faultedSubject = new ReplaySubject<Error>(1);

        protected TState _lastState;

        public Store(Reducer<TState> reducer, TState initialState = default(TState), params Middleware<TState>[] middlewares)
        {
            
            _reducer = reducer;
            _dispatcher = ApplyMiddlewares(middlewares);

            _lastState = initialState;
            _stateSubject.OnNext(_lastState);
        }

        public IAction Dispatch(IAction action,IProgress<int> progress=null)
        {
            return _dispatcher(action,progress);
        }

        public TState GetState()
        {
            return _lastState;
        }
        
        public IDisposable Subscribe(IObserver<TState> observer)
        {
            
            return _stateSubject
                .Subscribe(observer);
        }
        public IDisposable SubscribeForErrors(IObserver<Error> observer)
        {
            return _faultedSubject
                .Subscribe(observer);
        }
        private Dispatcher ApplyMiddlewares( params Middleware<TState>[] middlewares)
        {
            Dispatcher dispatcher = InnerDispatch;
            foreach (var middleware in middlewares)
            {
                dispatcher = middleware(this)(dispatcher);
            }
            return dispatcher;
        }

        private IAction InnerDispatch(IAction action, IProgress<int> progress)
        {
            lock (_syncRoot)
            {
                _lastState = _reducer(_lastState, action,progress);
            }
            _stateSubject.OnNext(_lastState);
            return action;
        }
    }
}
