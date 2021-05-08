// I'm not really proficient with JS, please forgive me
function fetchlogs() {
    console.log("Fetching logs");
    fetch("/logs", {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
        }}).catch(function (error) {
            console.warn("Error: " + error);
            var mainNode = document.getElementById("log-mount");
            mainNode.innerHTML = "";
            var node = document.createElement("pre");
            node.className = "log-entry";
            node.innerHTML = "Failed to retrieve logs";
            node.className = "log-entry log-error";
            node.style.backgroundColor = "transparent";
            mainNode.appendChild(node);
        }).then(function (response) {
            response.json().then(function(value) {
                console.log("Fetch done");
                console.log(value);
                // Now add the elements
                addlogs(value)
        })
    });
}
function addlogs(json) {
    var mainNode = document.getElementById("log-mount");
    mainNode.innerHTML = "";
    json.reverse();
    json.forEach(element => {
        var log = element.Message;
        
        // The entry
        var node = document.createElement("pre");
        
        // Log entry content
        var severity = document.createElement("pre");
        var source = document.createElement("pre")
        var message = document.createElement("pre")
        
        // Assign the entry it's class
        node.className = "log-entry";
        
        // Make some of the content have a minimum width
        severity.style = "min-width: 100px;";
        source.style = "min-width: 250px;";

        // Determine the severity
        switch (log.Severity) {
            case 5:
                severity.innerHTML = "Debug";
                severity.className += " log-severity-debug";
                break;
            case 4:
                severity.innerHTML = "Verbose";
                severity.className += " log-severity-verbose";
                break;
            case 3:
                severity.innerHTML = "Info";
                severity.className += " log-severity-info";
                break;
            case 2:
                severity.innerHTML = "Warning";
                severity.className += " log-severity-warn";
                break;
            case 1:
                severity.innerHTML = "Error";
                severity.className += " log-severity-error";
                break;
            case 0:
                severity.innerHTML = "Critical";
                severity.className += " log-severity-critical";
                severity.style = "animation-delay: .5s;";
                break;
        }
        
        // Get the source
        source.innerHTML = log.Source;

        // Set the message of the log
        message.innerHTML = log.Message;
        
        // Now add the children to the log entry
        node.appendChild(severity);
        node.appendChild(source);
        node.appendChild(message);
        
        // Add the log entry to the container
        mainNode.appendChild(node);
    });
}

function stopbot() {
    var buttonid = "stopbutton";
    
    changebutton(buttonid, "Stopping bot...");
    
    fetch("/stop", {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(function name() {
        changebutton(buttonid, "Bot stopped");
        changebutton("killbutton", "Bot stopped");
    }).catch(function error(error) {
        changebutton(buttonid, "Failed to stop bot!");
        console.warn("Failed to stop bot!", error);
    });
}
function killbot() {
    var buttonid = "killbutton";
    
    changebutton(buttonid, "Stopping bot...");
    
    fetch("/kill", {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    }).then(function name() {
        changebutton(buttonid, "Bot stopped");
        changebutton("stopbutton", "Bot stopped");
    }).catch(function error(error) {
        changebutton(buttonid, "Failed to stop bot!");
        console.warn("Failed to kill bot!", error);
    });
}
function changebutton(id, text) {
    document.getElementById(id).onclick = "";
    document.getElementById(id).children[0].innerHTML = text;
    document.getElementById(id).className = "button red deactivated";
}