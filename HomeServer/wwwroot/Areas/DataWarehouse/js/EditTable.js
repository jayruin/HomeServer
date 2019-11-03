var definition = document.getElementById("definition");
var definitionButton = document.getElementById("definition-button");
function toggleDefinition() {
    if (definition.style.display === "block") {
        definition.style.display = "none";
    }
    else {
        definition.style.display = "block";
    }
}
definitionButton.addEventListener("click", toggleDefinition);

var addRowInputs = document.querySelectorAll("table thead tr th input");
function clear() {
    for (let i = 0; i < addRowInputs.length; i++) {
        addRowInputs[i].value = "";
    }
}
var clearButton = document.getElementById("clear-button");
clearButton.addEventListener("click", clear);

var hiddenInputs = document.querySelectorAll("#hiddenInputs input");

var deleteButtons = document.querySelectorAll(".delete-button");
var editButtons = document.querySelectorAll(".edit-button");

function submitForm(e) {
    var button = e.target;
    var formID = button.form.id;
    var tr = button.parentNode.parentNode;
    var items = tr.querySelectorAll("td:not(.td-button):not(.td-input)");
    for (let i = 0; i < items.length; i++) {
        hiddenInputs[i].setAttribute("form", formID);
        hiddenInputs[i].value = items[i].textContent;
    }
    document.getElementById(formID).submit();
}

for (let i = 0; i < deleteButtons.length; i++) {
    deleteButtons[i].addEventListener("click", submitForm);
}

var rowEditing = null;

function cancelEdit() {
    var inputs = rowEditing.querySelectorAll("td input");
    var temps = rowEditing.querySelectorAll(".temp");
    for (let i = 0; i < inputs.length; i++) {
        rowEditing.removeChild(inputs[i].parentNode);
    }
    for (let i = 0; i < temps.length; i++) {
        rowEditing.removeChild(temps[i].parentNode);
    }
    for (let i = 0; i < rowEditing.children.length; i++) {
        rowEditing.children[i].style.display = "table-cell";
    }
    rowEditing = null;
}

function startEdit(e) {
    if (rowEditing) {
        cancelEdit();
    }
    var button = e.target;
    rowEditing = button.parentNode.parentNode;
    var rowLength = rowEditing.children.length;
    var td;
    for (let i = 0; i < rowLength; i++) {
        rowEditing.children[i].style.display = "none";
    }
    for (let i = 0; i < rowLength - 2; i++) {
        let input = document.createElement("input");
        input.type = "text";
        input.placeholder = addRowInputs[i].placeholder;
        input.setAttribute("form", "edit-row");
        input.value = (rowEditing.children[i].textContent === "NULL" ? "" : rowEditing.children[i].textContent);
        input.name = `newRow[${i}]`;
        td = document.createElement("td");
        td.className = "td-input";
        td.appendChild(input);
        rowEditing.appendChild(td);
    }

    var submitEditButton = document.createElement("button");
    submitEditButton.type = "button";
    submitEditButton.className = "edit-button temp";
    submitEditButton.setAttribute("form", "edit-row");
    submitEditButton.textContent = "Go";
    submitEditButton.addEventListener("click", submitForm);
    td = document.createElement("td");
    td.className = "td-button";
    td.appendChild(submitEditButton);
    rowEditing.appendChild(td);

    var cancelEditButton = document.createElement("button");
    cancelEditButton.type = "button";
    cancelEditButton.className = "delete-button temp";
    cancelEditButton.textContent = "Cancel";
    cancelEditButton.addEventListener("click", cancelEdit)
    td = document.createElement("td");
    td.className = "td-button";
    td.appendChild(cancelEditButton);
    rowEditing.appendChild(td);

}

for (let i = 0; i < editButtons.length; i++) {
    editButtons[i].addEventListener("click", startEdit);
}
