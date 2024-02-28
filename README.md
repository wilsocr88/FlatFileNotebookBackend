# Flat File Storage API

A dead-simple API for storage of small notes in flat files.

## GET: File/GetList?name=filenamehere

Returns the full list of stored items in given file

## GET: File/CreateList?name=filenamehere

Creates a new file with given name

## POST: File/SaveItem

Save an item to storage

Body:

| Name  | Type   | Description                      |
| ----- | ------ | -------------------------------- |
| file  | string | Name of file to save new item in |
| title | string | Title of new item                |
| body  | string | Body of new item                 |

## POST: File/EditItem

Body:

| Name  | Type   | Description                             |
| ----- | ------ | --------------------------------------- |
| index | int    | Index of item we're editing in the list |
| file  | string | Name of file we're editing an item in   |
| title | string | Edited title of item                    |
| body  | string | Edited body of item                     |
