namespace ProductivityApp.Services;

public class UndoStackService
{
    private readonly object _sync = new();
    private readonly Dictionary<int, Stack<int>> _deletedTaskStacks = [];

    public void PushDeletedTask(int userId, int taskId)
    {
        lock (_sync)
        {
            if (!_deletedTaskStacks.TryGetValue(userId, out var stack))
            {
                stack = new Stack<int>();
                _deletedTaskStacks[userId] = stack;
            }

            stack.Push(taskId);
        }
    }

    public int? PopDeletedTask(int userId)
    {
        lock (_sync)
        {
            if (!_deletedTaskStacks.TryGetValue(userId, out var stack) || stack.Count == 0)
            {
                return null;
            }

            return stack.Pop();
        }
    }

    public bool HasDeletedTask(int userId)
    {
        lock (_sync)
        {
            return _deletedTaskStacks.TryGetValue(userId, out var stack) && stack.Count > 0;
        }
    }
}
