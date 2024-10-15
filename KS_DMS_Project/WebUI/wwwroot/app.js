const apiUrl = 'http://localhost:8081'; // before /MyDoc

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
                    <span class="block"> <strong>Title:</strong> ${myDoc.title} </span>
                    <span class="block"> <strong>Author:</strong> ${myDoc.author} </span>
                    <span class="block"> <strong>TextField:</strong> ${myDoc.textField}</span>
                    <br>
                    <button class="delete" style="margin-left: 10px;" onclick="deleteTask(${myDoc.id})">Delete</button>
                `;
                DocList.appendChild(li);
            });
        })
        .catch(error => console.error('Fehler beim Abrufen der MyDoc-Items:', error));
}


// Function to add a new task
function addDoc() {
    const docTitle = document.getElementById('DocTitle').value;
    const docAuthor = document.getElementById('DocAuthor').value;
    const docTextfield = document.getElementById('DocTextField').value;

    const errorDiv = document.getElementById('ErrorMessages');//Div für Fehlermeldung

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
                    errorDiv.innerHTML = '<ul>' + Object.values(err.errors)
                        .map(e => `<li>${e}</li>`).jion('') + '</ul>'
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

    //const isComplete = document.getElementById('isComplete').checked;

    if (docTitle.trim() === '') {
        alert('Please enter an id');
        return;
    }

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
                response.json().then(err => alert("Fehler: " + err.message));
                console.error('Fehler beim Updaten des Docs.');
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

