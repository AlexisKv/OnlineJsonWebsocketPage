using System.Collections.Concurrent;

namespace OnlineJson.Manager;

public class EditorRoom
{
    public string Name { get; }
    public ConcurrentDictionary<string, string> Collaborators { get; } = new();
    private string _content = "";

    public EditorRoom(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name), "Room name cannot be null.");
        }

        Name = name;
    }

    public void UpdateContent(string content)
    {
        _content = content ?? throw new ArgumentNullException(nameof(content), "Content cannot be null.");
    }

    public string GetContent()
    {
        return _content;
    }


    public void Join(string collaboratorId, string collaboratorName)
    {
        if (string.IsNullOrWhiteSpace(collaboratorId))
        {
            throw new ArgumentNullException(nameof(collaboratorId),
                "Collaborator Id cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(collaboratorName))
        {
            throw new ArgumentNullException(nameof(collaboratorName),
                "Collaborator name cannot be null or whitespace.");
        }

        Collaborators.TryAdd(collaboratorId, collaboratorName);
    }
}