const content = document.getElementById("content");

async function LoadPage(page) {
    let div = document.createElement("div");

    let response = await fetch(`/pages/${page}.html`);
    let html = await response.text();

    div.innerHTML = html;
    div.classList.add("item");
    content.appendChild(div);
    window.dispatchEvent(new CustomEvent("display:refresh"));
}

LoadPage("home");