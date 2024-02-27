# Flat File Storage API

A dead-simple API with two endpoints:

## GET: Storage/GetAll
Returns the full list of stored items

## POST: Storage/SaveItem
Body:

| Name    | Type |
| -------- | ------- |
| title | _string_ |
| body | _string_ |