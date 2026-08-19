public interface IState
{
    void Enter();
    void Exit();
    void FixedUpdate();
}
public class FSM<T> where T : IState
{
    public T Current { get; private set; }
    public T Prev { get; private set; }
    public void SetState(T next)
    {
        if (Current.Equals(next)) return;
        Current?.Exit();
        Prev = Current;
        Current = next;
        Current.Enter();
    }
}