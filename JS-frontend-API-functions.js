import FetchPromise from "./FetchPromise";

export function getList(filename) {
    return FetchPromise({
        url: window["config"].apiUrl + "File/GetList?name=" + filename,
        method: "GET",
    });
}

export function createList(filename) {
    return FetchPromise({
        url: window["config"].apiUrl + "File/CreateList?name=" + filename,
        method: "GET",
    });
}

export function listFiles() {
    return FetchPromise({
        url: window["config"].apiUrl + "File/ListFiles",
        method: "GET",
    });
}

export function saveItem(file, title, body) {
    return FetchPromise({
        url: window["config"].apiUrl + "File/SaveItem",
        method: "POST",
        body: {
            file: file,
            title: title,
            body: body,
        },
    });
}

export function editItem(id, file, title, body) {
    return FetchPromise({
        url: window["config"].apiUrl + "File/EditItem",
        method: "",
        body: {
            id: id,
            file: file,
            title: title,
            body: body,
        },
    });
}

export function deleteItem(id, file) {
    return FetchPromise({
        url: window["config"].apiUrl + "File/DeleteItem",
        method: "DELETE",
        body: {
            id: id,
            file: file,
        },
    });
}

export function deleteList(filename) {
    return FetchPromise({
        url: window["config"].apiUrl + "File/DeleteList?name=" + filename,
        method: "DELETE",
    });
}
