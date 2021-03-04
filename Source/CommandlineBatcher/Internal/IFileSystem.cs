namespace CommandlineBatcher.Internal
{
    public interface IFileSystem
    {
        string ReadAllText(string path);
    }
}
