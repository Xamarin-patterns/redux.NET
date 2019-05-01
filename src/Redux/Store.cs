using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Redux
{
    public delegate IAction Dispatcher(IAction action);

    public delegate TState Reducer<TState>(TState previousState, IAction action);

    public delegate Func<Dispatcher, Dispatcher> Middleware<TState>(IStore<TState> store);

    public interface IStore<TState> : IObservable<TState>
    {
        IAction Dispatch(IAction action);

        TState GetState();
    }

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
    public class AsyncStore<TState> : Store<Task<TState>>
    {
        public AsyncStore(Reducer<Task<TState>> reducer, Task<TState> initialState = null)
            : base(reducer, initialState)
        {
        }
        public Task DispatchAsync(IAction action)
        {
            var lastState = _reducer.Invoke(_lastState, action);
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

        public IAction Dispatch(IAction action)
        {
            return _dispatcher(action);
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
        private Dispatcher ApplyMiddlewares(params Middleware<TState>[] middlewares)
        {
            Dispatcher dispatcher = InnerDispatch;
            foreach (var middleware in middlewares)
            {
                dispatcher = middleware(this)(dispatcher);
            }
            return dispatcher;
        }

        private IAction InnerDispatch(IAction action)
        {
            lock (_syncRoot)
            {
                _lastState = _reducer(_lastState, action);
            }
            _stateSubject.OnNext(_lastState);
            return action;
        }
    }
}
