namespace S_Machine
{

    public interface IState<T>
    {
        void OnEnter(T context);
        void OnUpdate(T context);
        void OnExit(T context);
    }
}