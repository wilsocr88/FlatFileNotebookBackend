# Flat File Notebook Backend

## Use

Click `login` on the front end once (i.e., call the Auth/Login endpoint) to create a default account using the credentials from `config.json`.

# API

## File

### GET: File/GetList?name=filenamehere

Returns the full list of stored items in given file

### GET: File/CreateList?name=filenamehere

Creates a new file with given name

### GET: File/ListFiles

List all files that currently exist.
Skip any files that start with a dot `.` character.

### POST: File/SaveItem

Save an item to storage

Body:

| Name  | Type   | Description                      |
| ----- | ------ | -------------------------------- |
| File  | string | Name of file to save new item in |
| Title | string | Title of new item                |
| Body  | string | Body of new item                 |

### POST: File/EditItem

Edit a specific item id in a specific file

Body:

| Name  | Type   | Description                             |
| ----- | ------ | --------------------------------------- |
| Id    | int    | Index of item we're editing in the list |
| File  | string | Name of file we're editing an item in   |
| Title | string | Edited title of item                    |
| Body  | string | Edited body of item                     |

### POST: File/ReorderItem

Reorder the list of items in a file

Body:

| Name       | Type   | Description                                         |
| ---------- | ------ | --------------------------------------------------- |
| File       | string | Name of file we're reordering in                    |
| CurrentPos | int    | Current (old) position of the item we're reordering |
| NewPos     | int    | New position of the item                            |

### DELETE: File/DeleteItem

Delete an item in a file

Body:

| Name | Type   | Description                      |
| ---- | ------ | -------------------------------- |
| Id   | int    | Index of item we're deleting     |
| File | string | Name of file we're deleting from |

### DELETE: File/DeleteList?name=filenamehere

Delete entire list/file

## Auth

### GET: Auth/CheckAuth

Check auth token

### POST: Auth/Login

Log in with username and password

Body:

| Name | Type   | Description |
| ---- | ------ | ----------- |
| User | string | Username    |
| Pass | string | Password    |

### POST: Auth/CreateUser

Create a new user

Body:

| Name | Type   | Description |
| ---- | ------ | ----------- |
| User | string | Username    |
| Pass | string | Password    |
