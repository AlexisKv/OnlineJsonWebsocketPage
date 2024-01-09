using Microsoft.AspNetCore.SignalR;
using OnlineJson.Manager;

namespace OnlineJson.Hubs;

public class EditorHub : Hub
{
    public static EditorManager _manager = new();

    public async Task<ICollection<string>> JoinFileRoom(string fileName, string collaboratorName)
    {
        var collaboratorId = Context.ConnectionId;
        var room = _manager.JoinRoom(fileName, collaboratorId, collaboratorName);

        await Groups.AddToGroupAsync(Context.ConnectionId, room.Name);

        // Notify all collaborators about the new user
        await Clients.Group(room.Name).SendAsync("UserJoined", collaboratorName);
        // Notify clients about the current collaborators
        await Clients.Group(room.Name).SendAsync("CollaboratorsUpdated", room.Collaborators);

        return room.Collaborators.Values;
    }

    public async Task GetUnchangedFile(string fileName)
    {
        var room = _manager.GetRoom(fileName);

        if (room.Collaborators.Count == 1)
        {
            string fileContent = await ReadFileContentAsync(fileName);
            room.UpdateContent(fileContent);
        }

        await Clients.Caller.SendAsync("FileUpdated", room.GetContent());
    }

    public async Task ResetFile(string fileName)
    {
        var room = _manager.GetRoom(fileName);

        string fileContent = await ReadFileContentAsync(fileName);
        room.UpdateContent(fileContent);

        await Clients.Group(room.Name).SendAsync("FileUpdated", room.GetContent());
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var nameWhoLeaving = _manager.GetNameWhoLeaves(Context.ConnectionId);
        var roomNameAndCollaboratorName = _manager.LeaveRooms(Context.ConnectionId);

        foreach (var room in roomNameAndCollaboratorName)
        {
            // await Clients.Group(room.Key).SendAsync("CollaboratorsUpdated", room.Value.Collaborators.Values);
            await Clients.Group(room.Key).SendAsync("LeavingRoomNotify", nameWhoLeaving.Value);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task EditFile(string fileName, string content)
    {
        var room = _manager.GetRoom(fileName);

        room.UpdateContent(content);

        await Clients.Group(room.Name).SendAsync("FileUpdated", room.GetContent());
    }

    private async Task<string> ReadFileContentAsync(string fileName)
    {
        string filePath = Path.Combine("wwwroot/jsonFiles", fileName); // Adjust the path accordingly

        try
        {
            // Read the content of the file
            string fileContent = await File.ReadAllTextAsync(filePath);

            return fileContent;
        }
        catch (Exception ex)
        {
            // Handle exceptions, log errors, etc.
            Console.WriteLine($"Error reading file content: {ex.Message}");
            return string.Empty; // or throw an exception if appropriate
        }
    }

    public async Task DownloadFile(string fileName)
    {
        var room = _manager.GetRoom(fileName);

        // Get the current content of the file
        var content = room.GetContent();

        // Send the file content to the client for download
        await Clients.Caller.SendAsync("DownloadFile", fileName, content);
    }

    public async Task GetCollaborators(string fileName)
    {
        var room = _manager.GetRoom(fileName);

        // Send the current collaborators to the client
        await Clients.Caller.SendAsync("CollaboratorsUpdated", room.Collaborators.Values);
    }

    public Task<List<string?>> GetFileList()
    {
        var fileList = _manager.GetFileList();
        return Task.FromResult(fileList);
    }
    
    
}