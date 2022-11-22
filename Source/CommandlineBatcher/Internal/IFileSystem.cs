using System.Text;

namespace CommandlineBatcher.Internal;

public interface IFileSystem
{
    string ReadAllText(string path);

    bool FileExists(string path);

    string GetCurrentDirectory();
    
    void AppendAllText(string path, string content, Encoding encoding);

    void WriteAllText(string path, string content, Encoding encoding);
}