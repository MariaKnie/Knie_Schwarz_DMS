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
                const li = document.createElement('li');
                li.id = `doc-${myDoc.id}`; // Set unique id for each <li>

                li.style.borderRadius = '8px'; // Rounded corners
                li.style.boxShadow = '0px 4px 6px rgba(0, 0, 0, 0.5)'; // Subtle shadow
                li.style.padding = '15px'; // Padding inside the card
                li.style.marginBottom = '10px'; // Space between cards

                li.innerHTML = `
                    <button class="edit" style="margin-left: 10px; float: right;" onclick="openEditPrompt(${myDoc.id}, '${myDoc.title}', '${myDoc.author}', '${myDoc.textfield}')">Edit</button>
                    <span class="block"> <strong>ID:</strong> ${myDoc.id} </span>
                    <span class="block"> <strong>CreateDate:</strong> ${myDoc.createddate} </span>
                    <span class="block"> <strong>EditDate:</strong> ${myDoc.editeddate} </span>
                    <span class="block"> <strong>Title:</strong> ${myDoc.title} </span>
                    <span class="block"> <strong>Author:</strong> ${myDoc.author} </span>
                    <span class="block"> <strong>TextField:</strong> ${myDoc.textfield}</span>
                    <span class="block"> <strong>OcrText:</strong> ${myDoc.ocrtext || "No ocrText"}</span>
                    <span class="block"> <strong>Filename:</strong> ${myDoc.filename || "No file"}</span>
                    <br/>
                    <input type="file" id="fileInput${myDoc.id}" />
                    <button style="margin-left: 10px;" onclick="uploadFile(${myDoc.id}, document.getElementById('fileInput${myDoc.id}'))">Upload File</button>
                    ${myDoc.filename ? `
                        <button class="delete" style="margin-left: 10px;" onclick="deleteFile(${myDoc.id}, '${myDoc.filename}')">Delete File</button>
                        <button class="download" style="margin-left: 10px;" onclick="downloadFile(${myDoc.id}, '${myDoc.filename}')">Download File</button>
                    ` : ''}
                    <br/>
                    <button class="delete" style="margin-left: 10px;" onclick="deleteTask(${myDoc.id})">Delete</button>
                `;
                DocList.appendChild(li);
            });
        })
        .catch(error => console.error('Error fetching MyDoc items:', error));
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
    errorDiv.innerHTML = "";

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



// Function to filter and display documents
function filterDocList() {
    const searchValue = document.getElementById('searchBar').value.trim().toLowerCase();
    const allItems = document.querySelectorAll('#myDocList li');
    const DocList = document.getElementById('myDocList');
    const filtertext = document.getElementById('filter');

    // Hide all list items initially
    allItems.forEach(item => {
        item.style.display = 'none';
    });

    filtertext.innerHTML = "";

    if (searchValue) {
        fetch(`${apiUrl}/search/querystring`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(searchValue) // Wrap the search term in an object
        })
            .then(response => response.json())
            .then(data => {

                console.log(data.stringify)

                if (data.length === 0) {
                    filtertext.innerHTML = '<p>No results found.</p>';
                } else if (data.message) {
                    // Handle the case when nothing is found
                    filtertext.innerHTML = '<p>No results found.</p>';
                } else {
                    // Loop over search results and display matching items
                    data.forEach(resultDoc => {
                        const matchingItem = document.getElementById(`doc-${resultDoc.id}`);
                        if (matchingItem) {
                            matchingItem.style.display = ''; // Show the matching item
                        }
                    });
                }
            })
            .catch(error => {
                console.error('Error fetching search results:', error);
                alert('Error fetching search results');
            });
    } else {
        // Show all items if search bar is empty
        allItems.forEach(item => {
            item.style.display = '';
        });
    }
}




// Function to open a prompt for editing
function openEditPrompt(id, title, author, textfield) {
    // Create an overlay
    const overlay = document.createElement('div');
    overlay.style.position = 'fixed';
    overlay.style.top = '0';
    overlay.style.left = '0';
    overlay.style.width = '100%';
    overlay.style.height = '100%';
    overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.5)'; // Semi-transparent black
    overlay.style.zIndex = '999';

    // Create a modal
    const modal = document.createElement('div');
    modal.style.position = 'fixed';
    modal.style.top = '50%';
    modal.style.left = '50%';
    modal.style.transform = 'translate(-50%, -50%)';
    modal.style.background = '#525252'; // Grey background
    modal.style.padding = '20px';
    modal.style.borderRadius = '8px';
    modal.style.boxShadow = '0px 4px 6px rgba(0, 0, 0, 0.1)';
    modal.style.zIndex = '1000';
    modal.style.width = '400px';

    modal.innerHTML = `
        <h3 class="mb-3">Add or Update Doc</h3>
        <form>
            <!-- Error Div -->
            <div id="errorMessage" class="text-danger"></div>

            <div class="mb-3">
                <label for="editTitle" class="form-label">Title</label>
                <input type="text" class="form-control" id="editTitle" value="${title}" placeholder="Title">
            </div>
            <div class="mb-3">
                <label for="editAuthor" class="form-label">Author</label>
                <input type="text" class="form-control" id="editAuthor" value="${author}" placeholder="Author">
            </div>
            <div class="mb-3">
                <label for="editTextfield" class="form-label">TextField</label>
                <textarea class="form-control" id="editTextfield" placeholder="TextField" rows="3">${textfield}</textarea>
            </div>

            <button type="button" class="btn btn-primary" id="submitEdit">Submit</button>
            <button type="button" class="btn btn-secondary" id="cancelEdit">Cancel</button>
        </form>
    `;

    // Append overlay and modal
    document.body.appendChild(overlay);
    document.body.appendChild(modal);

    // Handle submit
    document.getElementById('submitEdit').addEventListener('click', () => {
        const updatedTitle = document.getElementById('editTitle').value;
        const updatedAuthor = document.getElementById('editAuthor').value;
        const updatedTextfield = document.getElementById('editTextfield').value;

        const updatedDoc = {
            id: id,
            title: updatedTitle,
            author: updatedAuthor,
            Textfield: updatedTextfield
        };

        fetch(`${apiUrl}/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(updatedDoc)
        })
            .then(response => {
                if (response.ok) {
                    fetchMyDocItems(); // Refresh the list
                    alert('Document updated successfully!');
                } else {
                    alert('Error updating the document.');
                }
            })
            .catch(error => console.error('Error:', error));

        // Remove modal and overlay after submission
        document.body.removeChild(modal);
        document.body.removeChild(overlay);
    });

    // Handle cancel
    document.getElementById('cancelEdit').addEventListener('click', () => {
        document.body.removeChild(modal);
        document.body.removeChild(overlay);
    });
}
