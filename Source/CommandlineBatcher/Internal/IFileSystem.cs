namespace CommandlineBatcher.Internal
{
    public interface IFileSystem
    {
        string ReadAllText(string path);

        bool FileExists(string path);
    }
}
