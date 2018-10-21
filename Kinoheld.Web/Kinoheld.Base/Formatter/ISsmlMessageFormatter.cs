namespace Kinoheld.Base.Formatter
{
    public interface ISsmlMessageFormatter<in TData> where TData : class
    {
        string Format(TData overview);
    }
}