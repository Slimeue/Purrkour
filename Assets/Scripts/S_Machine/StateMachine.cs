namespace S_Machine 
{
    public class StateMachine<T>
    {
        private IState<T> _currentState;
        private readonly T _context;

        public StateMachine(T context)
        {
            this._context = context;
        }

        public void ChangeState(IState<T> newState)
        {
            _currentState?.OnExit(_context);
            _currentState = newState;
            _currentState?.OnEnter(_context);
        }

        public void Update()
        {
            _currentState?.OnUpdate(_context);
        }

        public IState<T> CurrentState => _currentState;
    }
}
