# Tests

## Postman

We maintain a [postman collection](postman-tests/MALT-Api-Search-And-Add-Users.postman_collection.json), [postman environment](malt-local-env.postman_environment.json) and an external datafile with valid/invalid data to pass on to the backend requests.

To run the collection in Postman app

```bash
Select the folder to run within the collection and the datafile to feed.
```

You can also run the collection using [newman](https://www.npmjs.com/package/newman)

Install newman as a global tool

```bash
npm install -g newman
```

Run the collection example

```bash
cd postman/tests
newman run MALT-Api-Search-And-Add-Users.postman_collection.json -e malt-local-env.postman_environment.json
```

Run the collection with datafile example

```bash
cd tests/postman
newman run MALT-Api-Search-And-Add-Users.postman_collection.json -e malt-local-env.postman_environment.json -d get-user-data-file.json
```
