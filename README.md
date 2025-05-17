# Reedme

The `idgen` is an API to generate Ids with the following characteristics:

- Short: currently fixed at 6 characters
- Alphanumeric: IDs contain both uppercase and lowercase letters, and digits
- Unique: each Id is guaranteed to be unique
- Random sequence

## Preview

Try it live at: [https://id.aslanta.com](https://id.aslanta.com).

Request:

```code
GET https://id.aslanta.com
```

Response:

```JSON
{
    "id": "P5jtoa"
}
```

Feel free to use it in your demo apps, prototypes, or internal tools. There is limit of 200 requests per minute, reach out if you need more :rocket:

## How it works

The `idgen` has two main components. A cronjob generating the short Ids and storing them in a PostgreSQL database, and a web API loading the Ids.

The database has two tables: `ShortIdsHistory` to guaranteed global unicity, and `ShortIds` with the unique Ids available to be consumed from the API.

The cronjob adds the new Ids to both tables. And then the API reads the Ids from `ShortIds` in small batches cached in memory. The Ids are deleted right after they are loaded in memory to guarantee that they cannot be retrieved again.
