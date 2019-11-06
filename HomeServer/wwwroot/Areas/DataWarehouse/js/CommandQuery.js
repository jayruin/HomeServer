function submit(e) {
    document.getElementById("value").setAttribute("form", "submit-form");
    document.getElementById("submit-form").submit();
}

function saveItem(e) {
    document.getElementById("key").setAttribute("form", "save-form");
    document.getElementById("value").setAttribute("form", "save-form");
    document.getElementById("save-form").submit();
}

function deleteItem(e) {
    document.getElementById("delete-form-name").value = e.target.parentNode.parentNode.querySelector("a").textContent;
    console.log(e.target.parentNode.parentNode.querySelector("a").textContent);
    document.getElementById("delete-form").submit();
}

document.getElementById("submit-button").addEventListener("click", submit);
document.getElementById("save-button").addEventListener("click", saveItem);
var deleteButtons = document.querySelectorAll(".delete-button");
for (let i = 0; i < deleteButtons.length; i++) {
    deleteButtons[i].addEventListener("click", deleteItem);
}