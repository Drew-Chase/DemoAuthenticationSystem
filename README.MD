# Demo Authentication System
This project is meant to teach how to create a simple authentication system in C# using SQL.  For simplicity, we use SQLite in this project to store the user data, however this will work with most SQL based databases.

## Dependencies
- [SQLite](https://www.nuget.org/packages/System.Data.SQLite/) - This is used as the primary database for storing user data.
- [Chase.CommonLibrary](https://www.nuget.org/packages/Chase.CommonLib/) - This is a library that I created to help with common tasks in C#. We are primarily using the Crypt class to hash the passwords.
- [HashIds](https://www.nuget.org/packages/Hashids.Net/) - This is used to generate the user's unique ID. This is optional and can be replaced with any other method of generating a unique ID.
## Features
- Basic user management (creation, deletion, and updating)
- Password hashing
- User authentication
- Client facing tokens for re-authentication without needing to store the password

## Token Generation
The token is generated using the user's data. This is then hashed using AES256 to create a unique token that can be used to re-authenticate the user without needing to store the password. This token is not stored in the database is a new one is generated everytime the user logs in.
The token is then returned to the client and stored in a cookie. This cookie is then used to re-authenticate the user when they return to the site.    
The way we create the token is by taking the username, password, email and any other information that is unique to the user as well as some data that is unique to the connection or machine, like the IP Address or Machine name (depending on the environment) and hashing it using AES256. When the client returns, they send the token to the server and the server can then re-hash the information and compare it to the stored token. If they match, the user is authenticated. The unique connection data is used to ensure that the token is only valid for the current connection and cannot be used by someone else.