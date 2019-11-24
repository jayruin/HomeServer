var tbody = document.getElementById("tbody");
var button = document.getElementById("AddColumn");
var count = 0;

function getDataTypes() {
    let datatypes = [];

    let datainteger = document.createElement("option");
    datainteger.text = "INTEGER";
    datainteger.value = "INTEGER";

    let datatext = document.createElement("option");
    datatext.text = "TEXT";
    datatext.value = "TEXT";

    let datareal = document.createElement("option");
    datareal.text = "REAL";
    datareal.value = "REAL";

    let datablob = document.createElement("option");
    datablob.text = "BLOB";
    datablob.value = "BLOB";

    datatypes.push(datainteger);
    datatypes.push(datatext);
    datatypes.push(datareal);
    datatypes.push(datablob);

    return datatypes;
}



function addColumn() {
    let datatypes = getDataTypes();
    let tr = document.createElement("tr");

    let column = document.createElement("input");
    let type = document.createElement("select");
    let primaryKey = document.createElement("input");
    let notNull = document.createElement("input");
    let unique = document.createElement("input");
    let deleteButton = document.createElement("button");

    column.type = "text";
    for (let i = 0; i < datatypes.length; i++) {
        type.appendChild(datatypes[i]);
    }
    primaryKey.type = "checkbox";
    primaryKey.value = "true";
    notNull.type = "checkbox";
    notNull.value = "true";
    unique.type = "checkbox";
    unique.value = "true";
    deleteButton.type = "button";
    deleteButton.textContent = "Delete";
    deleteButton.addEventListener("click", deleteColumn);

    column.name = `tableColumns[${count}].Column`;
    type.name = `tableColumns[${count}].Type`;
    primaryKey.name = `tableColumns[${count}].Primarykey`;
    notNull.name = `tableColumns[${count}].NotNull`;
    unique.name = `tableColumns[${count}].Unique`;

    let tdColumn = document.createElement("td");
    let tdType = document.createElement("td");
    let tdPrimaryKey = document.createElement("td");
    let tdNotNull = document.createElement("td");
    let tdUnique = document.createElement("td");
    let tdDeleteButton = document.createElement("td");

    tdColumn.appendChild(column);
    tdType.appendChild(type);
    tdPrimaryKey.appendChild(primaryKey);
    tdNotNull.appendChild(notNull);
    tdUnique.appendChild(unique);
    tdDeleteButton.appendChild(deleteButton);

    tr.appendChild(tdColumn);
    tr.appendChild(tdType);
    tr.appendChild(tdPrimaryKey);
    tr.appendChild(tdNotNull);
    tr.appendChild(tdUnique);
    tr.appendChild(tdDeleteButton);

    let hidden = document.createElement("input");
    hidden.type = "hidden";
    hidden.name = "tableColumns.Index";
    hidden.value = `${count}`;

    tr.appendChild(hidden);

    tbody.appendChild(tr);

    count += 1;
}

function deleteColumn(e) {
    tbody.removeChild(e.target.parentNode.parentNode);
}

button.addEventListener("click", addColumn);