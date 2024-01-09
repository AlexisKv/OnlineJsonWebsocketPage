// editor.js
(function () {
   let connection = new signalR.HubConnectionBuilder().withUrl("/editorHub").build();
    let currentFileName = "";
    let contentBox = "";
    let collaboratorName = "";
    let isUpdating = false;
    let changeSource = null

    let editor = CodeMirror(document.getElementById("json-editor"), {
        lineNumbers: true,
        mode: "application/ld+json",
        matchBrackets: true
    });
    
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

    $("#name-input").keyup(function (event) {
        // check if the key pressed was "Enter"
        if (event.key === "Enter" || event.keyCode === 13) {
            $("#enter-button").click();
            
            const fileName = $("#file-list").val();
            currentFileName = fileName;
            connection.invoke("JoinFileRoom", fileName, collaboratorName);

            lastSentContent = ''; // reset lastSentContent

            switchToEditingView();// simulate a click on the enter button
        }
    });

    function animateCollaboratorEntry(collabElem) {
        // Use GSAP to animate
        gsap.from(collabElem, { duration: 1, x: "+=150", autoAlpha: 0 });
    }


    connection.on("UserJoined", (newUserName) => {
        handleUserJoinedNotification(newUserName);
    });

    function handleUserJoinedNotification(newUserName) {

        // Get the #collaborators element
        let collabsDiv = document.querySelector("#collaborators");

        // Create a new collaborator element
        let collabElem = document.createElement('span');
        collabElem.id = newUserName.replace(/ /g, "_").replace(/,/g, "");  // Assign the id
        collabElem.innerText = newUserName;

        // Append the new collaborator
        collabsDiv.appendChild(collabElem);

        // Animate the new collaborator entry
        animateCollaboratorEntry(collabElem);
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

    editor.on('change', editorInstance => {
        let newContentBox = editorInstance.getValue();
        if (isUpdating || newContentBox === contentBox) {
            return;
        }
        lastSentContent = newContentBox;
        contentBox = newContentBox;
        changeSource = 'self';
        connection.invoke("EditFile", currentFileName, contentBox);
    });

    connection.on("FileUpdated", content => {
        if (changeSource !== 'self') {
            isUpdating = true;
            editor.setValue(content);
            isUpdating = false;
        }
        changeSource = null;
    });
    
    connection.on("LeavingRoomNotify", nameWhoLeaving => {
        handleLeavingRoomNotification(nameWhoLeaving);
    });

    function handleLeavingRoomNotification(nameWhoLeaving) {
     
        changeSource = null;

        // Find collaborator span by name
        let leavingCollabElem = document.querySelector("#" + nameWhoLeaving.replace(/ /g, "_").replace(/,/g, ""));

        if (leavingCollabElem) {
            // Animate the name disappearing
            animateLeavingCollaborator(leavingCollabElem);
        }
    }

    function animateLeavingCollaborator(leavingCollabElem) {
        gsap.to(leavingCollabElem, {
            duration: 1,
            x: "+=150",
            autoAlpha: 0,
            onComplete: () => {
                const parentElement = leavingCollabElem.parentElement;

                // Remove the element from DOM after animation
                leavingCollabElem.remove();

                // Update the collaborators string without trailing commas
                const collaboratorsSpans = Array.from(parentElement.getElementsByTagName("span"));
                const newCollaboratorsString = collaboratorsSpans
                    .map(span => span.innerText)
                    .join(", ");

                // Update the collaborators string in the parent element
                parentElement.innerText = newCollaboratorsString;
            }
        });
    }


    connection.on("CollaboratorsUpdated", collaborators => {
        const newCollaboratorNames = Object.values(collaborators).map(name =>
            name === collaboratorName ? `${name} (You)` : name
        );

        // Get the #collaborators element
        let collabsDiv = document.querySelector("#collaborators");

        // First, clear all existing entries
        collabsDiv.innerHTML = "";

        // Now append new collaborators
        newCollaboratorNames.forEach((name, i, arr) => {
            let collabElem = document.createElement('span');
            collabElem.id = name.replace(/ /g, "_").replace(/,/g, "");  // Assign the id

            // Adding comma after each collaborator (except the last one)
            collabElem.innerText = name + (i < arr.length - 1 ? ", " : "");

            // Use GSAP to animate
            animateCollaboratorEntry(collabElem);

            collabsDiv.appendChild(collabElem);
        });
    });



    $("#download-button").click(() => connection.invoke("DownloadFile", currentFileName));

    $("#exit-button").click(() => {
        connection.invoke("LeaveFileRoom", currentFileName);
        switchToWelcomeView();
    });

    $("#reset-button").click(() => {
        lastSentContent = '';
        connection.invoke("ResetFile", currentFileName);
    });

    connection.on("DownloadFile", (fileName, content) => downloadFile(fileName, content));

    function switchToEditingView() {
        // Slide up the welcome view after a delay
        setTimeout(function () {
            $("#editor-welcome").slideUp(1000);
        }, 500);

        // Fade in the editing view and initialize JSON editor after the animation completes
        $("#editor-editing").fadeIn(2000, function () {
            // Your existing logic for invoking SignalR
            connection.invoke("GetCollaborators", currentFileName);
        });
    }

    function downloadFile(fileName, content) {
        const blob = new Blob([content], {type: 'application/json'});
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
    }
})();
