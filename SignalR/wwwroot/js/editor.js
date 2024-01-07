// editor.js
(function () {
    let connection = new signalR.HubConnectionBuilder().withUrl("/editorHub").build();
    let currentFileName = "";
    let contentBox = "";
    let collaboratorName = "";

    // Fetch the list of JSON files from the server and populate the dropdown
    function populateFileList() {
        connection.invoke("GetFileList")
            .then(files => {
                const fileListDropdown = $("#file-list");
                fileListDropdown.empty();
                files.forEach(file => {
                    fileListDropdown.append($("<option />").val(file).text(file));
                });
            })
            .catch(err => $("#editor-error").text(err));
    }

    connection.start().then(() => {
        populateFileList();
    }).catch(err => $("#editor-error").text(err));

    $("#enter-button").click(() => {
        collaboratorName = $("#name-input").val();
        if (!collaboratorName.trim()) {
            $("#name-error").show();
            return;
        }

        $("#name-error").hide();

        const fileName = $("#file-list").val();
        currentFileName = fileName;
        connection.invoke("JoinFileRoom", fileName, collaboratorName);
        connection.invoke("GetUnchangedFile", currentFileName);
        
        switchToEditingView();
    });

    connection.on("CollaboratorsUpdated", collaborators => {
        const collaboratorNames = Object.values(collaborators).map(name =>
            name === collaboratorName ? `${name} (You)` : name
        );
        $("#collaborators").text(collaboratorNames.join(', '));
    });

    $("#json-editor").on('input', () => {
        contentBox = $("#json-editor").val();
        connection.invoke("EditFile", currentFileName, contentBox);
    });

    $("#download-button").click(() => connection.invoke("DownloadFile", currentFileName));

    $("#exit-button").click(() => {
        connection.invoke("LeaveFileRoom", currentFileName);
        switchToWelcomeView();
    });

    connection.on("FileUpdated", content => {
        $("#json-editor").val(content);
    });

    $("#reset-button").click(() => {
        connection.invoke("ResetFile", currentFileName);
    });
    
    connection.on("DownloadFile", (fileName, content) => downloadFile(fileName, content));

    function switchToWelcomeView() {
        $("#editor-welcome").show();
        $("#editor-editing").hide();
    }

    function switchToEditingView() {
        $("#editor-welcome").hide();
        $("#editor-editing").show();
        connection.invoke("GetCollaborators", currentFileName);
    }

    function downloadFile(fileName, content) {
        const blob = new Blob([content], {type: 'application/json'});
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
    }
})();
