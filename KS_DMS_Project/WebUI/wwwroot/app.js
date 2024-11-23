const apiUrl = 'http://localhost:8081/mydoc'; // before /MyDoc

// Function to fetch and display MyDoc items
function fetchMyDocItems() {
    console.log('Fetching MyDoc items...');
    fetch(apiUrl)
        .then(response => response.json())
        .then(data => {
            const DocList = document.getElementById('myDocList');
            DocList.innerHTML = ''; // Clear the list before appending new items
            data.forEach(myDoc => {

                // Create list item with delete and toggle complete buttons
                const li = document.createElement('li'); //| Completed: ${task.isComplete} <button style="margin-left: 10px;" onclick="toggleComplete(${myDoc.id}, ${myDoc.isComplete}, '${myDoc.title}')">  Mark as ${ myDoc.isComplete ? 'Incomplete' : 'Complete' }
                li.innerHTML = `
                    <span class="block"> <strong>ID:</strong> ${myDoc.id} </span>
                    <span class="block"> <strong>CreateDate:</strong> ${myDoc.createddate} </span>
                    <span class="block"> <strong>EditDate:</strong> ${myDoc.editeddate} </span>
                    <span class="block"> <strong>Title:</strong> ${myDoc.title} </span>
                    <span class="block"> <strong>Author:</strong> ${myDoc.author} </span>
                    <span class="block"> <strong>TextField:</strong> ${myDoc.textfield}</span>
                    <br/>
                    <span>File: ${myDoc.filename || "No file uploaded"}</span>
                    <input type="file" id="fileInput${myDoc.id}" />
                    <button style="margin-left: 10px;" onclick="uploadFile(${myDoc.id}, document.getElementById('fileInput${myDoc.id}'))">
                        Upload File
                    </button>
                    ${myDoc.filename ? `
                        <button class="delete" style="margin-left: 10px;" onclick="deleteFile(${myDoc.id}, '${myDoc.filename}')">
                             Delete File
                        </button>
                        <button class="download" style="margin-left: 10px;" onclick="downloadFile(${myDoc.id}, '${myDoc.filename}')">
                             Download File
                        </button>
                        ` : ''}
                    <br/>
                    <button class="delete" style="margin-left: 10px;" onclick="deleteTask(${myDoc.id})">Delete</button>

                `;

                DocList.appendChild(li);
            });
        })
        .catch(error => console.error('Fehler beim Abrufen der MyDoc-Items:', error));
}

function uploadFile(Id, fileInput) {
    const file = fileInput.files[0];
    if (!file) {
        alert("Keine Datei ausgewählt.");
        return;
    }

    const formData = new FormData();
    formData.append('docFile', file);

    fetch(`${apiUrl}/${Id}/upload`, {
        method: 'PUT',
        body: formData
    })
        .then(response => {
            if (response.ok) {
                fetchMyDocItems();
                alert("Datei erfolgreich hochgeladen.");
            } else {
                alert("Fehler beim Hochladen der Datei.");
            }
        })
        .catch(error => {
            console.error('Fehler:', error);
        });
}

function downloadFile(id, fileName) {
    fetch(`${apiUrl}/download/${fileName}`, {
        method: 'GET',
    })
        .then(response => {
            if (response.ok) {
                // Create a blob link to trigger the download
                response.blob().then(blob => {
                    const link = document.createElement('a');
                    const url = window.URL.createObjectURL(blob);
                    link.href = url;
                    link.download = fileName;  // File name for the downloaded file
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    window.URL.revokeObjectURL(url);  // Clean up the URL object
                    console.log(`File ${fileName} downloaded successfully.`);
                });
            } else {
                console.error('Error downloading the file');
            }
        })
        .catch(error => console.error('Fehler:', error));
}


// Function to delete a file
function deleteFile(id, filename) {
    if (!filename) {
        console.error("No file to delete");
        return;
    }

    // Delete the file from the backend (DAL)
    fetch(`${apiUrl}/${id}/File`, {
        method: 'DELETE',
    })
        .then(response => {
            if (response.ok) {
                fetchMyDocItems(); 
                alert("File deleted successfully from DAL.");
            } else {
                console.error('Error while deleting the file from DAL.');
                alert("Error deleting the file from DAL.");
            }
        })
        .catch(error => {
            console.error('Fehler:', error);
            alert("An error occurred while deleting the file from DAL.");
        });

    // Delete file from storage (Minio)
    fetch(`${apiUrl}/delete/${filename}`, {
        method: 'DELETE',
    })
        .then(response => {
            if (response.ok) {
                alert("File deleted successfully from storage.");
            } else {
                console.error('Error while deleting the file from storage.');
                alert("Error deleting the file from storage.");
            }
        })
        .catch(error => {
            console.error('Fehler:', error);
            alert("An error occurred while deleting the file from storage.");
        });
}


// Function to add a new task
function addDoc() {
    const docTitle = document.getElementById('DocTitle').value;
    const docAuthor = document.getElementById('DocAuthor').value;
    const docTextfield = document.getElementById('DocTextField').value;

    const errorDiv = document.getElementById('errorMessage');//Div für Fehlermeldung

    //const isComplete = document.getElementById('isComplete').checked;

    if (docTitle.trim() === '') {
        alert('Please enter a doc title');
        return;
    }

    const newMyDoc = {
        title: docTitle,
        author: docAuthor,
        Textfield: docTextfield
    };
        //isComplete: isComplete

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newMyDoc)
    })
        .then(response => {
            if (response.ok) {
                fetchMyDocItems(); // Refresh the list after adding
                document.getElementById('DocTitle').value = ''; // Clear the input field
                document.getElementById('DocAuthor').value = ''; // Clear the input field
                document.getElementById('DocTextField').value = ''; // Clear the input field
                //document.getElementById('isComplete').checked = false; // Reset checkbox
            } else {
                // Neues Handling für den Fall eines Fehlers (z.B. leeres Namensfeld)
                response.json().then(err => {
                    errorDiv.innerHTML = `<ul>` +
                        Object.values(err.errors).map(e => `<li>${e}</li>`).join('')
                        + `</ul>`;
                });

                console.error('Fehler beim Hinzufügen der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}

function updateDoc() {
    const docId = document.getElementById('DocID').value;
    const docTitle = document.getElementById('DocTitle').value;
    const docAuthor = document.getElementById('DocAuthor').value;
    const docTextfield = document.getElementById('DocTextField').value;

    const errorDiv = document.getElementById('errorMessage');//Div für Fehlermeldung

    //const isComplete = document.getElementById('isComplete').checked;

    if (docTitle.trim() === '') {
        alert('Please enter a doc title');
        return;
    }

    const newMyDoc = {
        id: docId,
        title: docTitle,
        author: docAuthor,
        Textfield: docTextfield
    };
    //isComplete: isComplete
    var apiUrl_put = apiUrl;
    apiUrl_put += '/' + docId;
    fetch(apiUrl_put, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newMyDoc)
    })
        .then(response => {
            if (response.ok) {
                fetchMyDocItems(); // Refresh the list after adding
                document.getElementById('DocID').value = ''; // Clear the input field
                document.getElementById('DocTitle').value = ''; // Clear the input field
                document.getElementById('DocAuthor').value = ''; // Clear the input field
                document.getElementById('DocTextField').value = ''; // Clear the input field
                //document.getElementById('isComplete').checked = false; // Reset checkbox
            } else {
                // Neues Handling für den Fall eines Fehlers (z.B. leeres Namensfeld)
                response.json().then(err => {
                    errorDiv.innerHTML = `<ul>` +
                        Object.values(err.errors).map(e => `<li>${e}</li>`).join('')
                        + `</ul>`;
                });

                console.error('Fehler beim Update der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}


// Function to delete a task
function deleteTask(id) {
    fetch(`${apiUrl}/${id}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (response.ok) {
                fetchMyDocItems(); // Refresh the list after deletion
            } else {
                console.error('Fehler beim Löschen der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}

// Function to toggle complete status
function toggleComplete(id, isComplete, name) {
    // Aufgabe mit umgekehrtem isComplete-Status aktualisieren
    const updatedTask = {
        id: id,  // Die ID des Tasks
        name: name, // Der Name des Tasks
        isComplete: !isComplete // Status umkehren
    };

    fetch(`${apiUrl}/${id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(updatedTask)
    })
        .then(response => {
            if (response.ok) {
                fetchMyDocItems(); // Liste nach dem Update aktualisieren
                console.log('Erfolgreich aktualisiert.');
            } else {
                console.error('Fehler beim Aktualisieren der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}


// Load MyDoc items on page load
document.addEventListener('DOMContentLoaded', fetchMyDocItems);



// Toggle between Add and Update mode
function toggleUpdateMode() {
    const isUpdateChecked = document.getElementById('isUpdate').checked;
    const docButton = document.getElementById('docButton');
    const docIDField = document.getElementById('DocID');

    if (isUpdateChecked) {
        docButton.textContent = 'Update Doc';
        docIDField.style.display = 'block'; // Show the ID field when updating
    } else {
        docButton.textContent = 'Add Doc';
        docIDField.style.display = 'none'; // Hide the ID field when adding
    }
}

// Function to handle add or update based on checkbox
function submitDoc() {
    const isUpdateChecked = document.getElementById('isUpdate').checked;

    if (isUpdateChecked) {
        // Update doc logic
        const docID = document.getElementById('DocID').value;
        if (docID) {
            console.log('Updating doc with ID:', docID);
            updateDoc();
        } else {
            alert('Please enter a Doc ID to update');
        }
    } else {
        // Add doc logic
        console.log('Adding new doc');
        addDoc();
    }
}

