namespace SimpleMDB;

public class Result<T>
{
    public T? Value { get; }
    public Exception? Error { get; }
    public bool IsSuccess => Error == null;

    public Result(T value)
    {
        Value = value;
        Error = null;
    }

    public Result(Exception error)
    {
        Value = default;
        Error = error;
    }
}
