using System;
using System.Reactive.Subjects;

namespace Redux
{
    public class StateChangedArgs<TState>
    {
        public StateChangedArgs(TState state, IAction action)
        {
            State = state;
            Action = action;
        }

        public TState State { get; }
        public IAction Action { get; }

    }
    public class Store<TState> : IStore<TState>
    {
        private readonly object _syncRoot = new object();
        private readonly Dispatcher _dispatcher;
        protected readonly Reducer<TState> _reducer;
        protected readonly ReplaySubject<StateChangedArgs<TState>> _stateSubject = new ReplaySubject<StateChangedArgs<TState>>(1);
        protected readonly ReplaySubject<ActionDispatchingException> _faultedSubject = new ReplaySubject<ActionDispatchingException>(1);

        protected TState _lastState;

        public Store(Reducer<TState> reducer, TState initialState = default(TState), params Middleware<TState>[] middlewares)
        {
            
            _reducer = reducer;
            _dispatcher = ApplyMiddlewares(middlewares);

            _lastState = initialState;
            _stateSubject.OnNext(new StateChangedArgs<TState>(_lastState,null));
        }

        public IAction Dispatch(IAction action,IProgress<int> progress=null)
        {
            return _dispatcher(action,progress);
        }

        public TState GetState()
        {
            return _lastState;
        }
        
        public IDisposable Subscribe(IObserver<StateChangedArgs<TState>> observer)
        {
            
            return _stateSubject
                .Subscribe(observer);
        }
        public IDisposable SubscribeForErrors(IObserver<ActionDispatchingException> observer)
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
            _stateSubject.OnNext(new StateChangedArgs<TState>(_lastState,action));
            return action;
        }
    }
}
