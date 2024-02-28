# Flat File Storage API

A dead-simple API for storage of small notes in flat files.

## GET: File/GetList?name=filenamehere

Returns the full list of stored items in given file

## GET: File/CreateList?name=filenamehere

Creates a new file with given name

## GET: File/ListFiles

List all files that currently exist.
Skip any files that start with a dot `.` character.

## POST: File/SaveItem

Save an item to storage

Body:

| Name  | Type   | Description                      |
| ----- | ------ | -------------------------------- |
| file  | string | Name of file to save new item in |
| title | string | Title of new item                |
| body  | string | Body of new item                 |

## POST: File/EditItem

Edit a specific item id in a specific file

Body:

| Name  | Type   | Description                             |
| ----- | ------ | --------------------------------------- |
| index | int    | Index of item we're editing in the list |
| file  | string | Name of file we're editing an item in   |
| title | string | Edited title of item                    |
| body  | string | Edited body of item                     |

## DELETE: File/DeleteItem

Delete an item in a file

Body:

| Name  | Type   | Description                      |
| ----- | ------ | -------------------------------- |
| index | int    | Index of item we're deleting     |
| file  | string | Name of file we're deleting from |

## DELETE: File/DeleteList?name=filenamehere

Delete entire list/file
