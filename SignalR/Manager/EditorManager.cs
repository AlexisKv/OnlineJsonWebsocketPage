using System.Collections.Concurrent;

namespace SignalR.Manager
{
    public class EditorManager
    {
        private readonly ConcurrentDictionary<string, EditorRoom> _editorRooms =
            new ConcurrentDictionary<string, EditorRoom>();

        public EditorRoom JoinRoom(string fileName, string collaboratorId, string collaboratorName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName), "File name cannot be null.");
            }

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

            var room = _editorRooms.GetOrAdd(fileName, new EditorRoom(fileName));

            room.Join(collaboratorId, collaboratorName);

            return room;
        }

        public List<KeyValuePair<string, EditorRoom>> LeaveRooms(string collaboratorId)
        {
            var roomNameAndCollaboratorName = new List<KeyValuePair<string, EditorRoom>>();

            foreach (var room in _editorRooms)
            {
                if (room.Value.Collaborators.ContainsKey(collaboratorId))
                {
                    room.Value.Collaborators.Remove(collaboratorId, out _);
                    roomNameAndCollaboratorName.Add(new KeyValuePair<string, EditorRoom>(room.Key, room.Value));
                }
            }

            return roomNameAndCollaboratorName;
        }

        public KeyValuePair<string, string> GetNameWhoLeaves(string collaboratorId)
        {
            KeyValuePair<string,string> collaborator = default;
            
            foreach (var room in _editorRooms)
            {
                collaborator = room.Value.Collaborators.FirstOrDefault(x => x.Key == collaboratorId);
                return new KeyValuePair<string, string>(collaborator.Key, collaborator.Value);
            }

            return collaborator;
        }

        public EditorRoom GetRoom(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName), "File name cannot be null.");
            }

            _editorRooms.TryGetValue(fileName, out var room);
            return room;
        }

        public List<string?> GetFileList()
        {
            string directoryPath = "wwwroot/jsonFiles"; // Spell out the correct path
            List<string?> fileList = Directory.GetFiles(directoryPath, "*.json")
                .Select(Path.GetFileName)
                .ToList();

            return fileList;
        }
    }

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
}