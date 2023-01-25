function onAuthorChecked() {
    debugger;
    var authors = $("#authorCheckboxes input[type=checkbox]:checked").val();
    var books = $(".book-card");

    if (authors.length == 0) {
        for (var i = 0; i < books.length; i++) {
            books[i].style.display = "block";
        }
    } else {
        for (var i = 0; i < books.length; i++) {
            var bookAuthor = books[i].children[0].children[1].children[0].children[1].innerText;
            if (!authors.includes(bookAuthor)) {
                books[i].style.display = "none";
            }
        }
    }
}

function getAuthors(subcategoryId) {
    var authors = $("#authorCheckboxes input[type=checkbox]:checked").toArray().map(o => o.value);
    $.ajax({
        type: "GET",
        URL: "/GenresController/GetBooks/",
        data: { subcategoryId: subcategoryId, authors: authors },
        dataType: "html",
        success: function (result) {
            debugger;
            elem = document.getElementsByClassName("col-9");
            elem.innerHtml = result;
        },
        error: function () {
            alert("failed");
        }
    });
}