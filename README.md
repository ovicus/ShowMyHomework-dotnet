# ShowMyHomework .NET Core Library

[![Build Status](https://travis-ci.com/ovicus/ShowMyHomework-dotnet.svg?branch=master)](https://travis-ci.com/ovicus/ShowMyHomework-dotnet)
![Nuget](https://img.shields.io/nuget/v/Ovicus.ShowMyHomework)
![MIT](https://img.shields.io/github/license/ovicus/ShowMyHomework-dotnet)

This library allows using ShowMyHomework.co.uk API to retrieve assigned tasks.

## Getting Started
To start using `Ovicus.ShowMyHomework`, install the latest version from [Nuget](https://www.nuget.org/packages/Ovicus.ShowMyHomework/).

`PM> Install-Package Ovicus.ShowMyHomework`

To retrieve the pending tasks, create a new instance of `ShowMyHomeworkClient` and call the method `GetTodos()`.

```csharp
var client = new ShowMyHomeworkClient(accessToken);

var todos = await client.GetTodos();

foreach (var todo in todos)
{
   var subject = todo.Subject; 
   var title = todo.Title;
   var dueDate = todo.DueOn; 
}
```

If you need to generate an access token, the library `Ovicus.ShowMyHomework.Auth` can help with this. Install the latest version from [Nuget](https://www.nuget.org/packages/Ovicus.ShowMyHomework.Auth/).

`PM> Install-Package Ovicus.ShowMyHomework.Auth`

```csharp
var authService = new AuthenticationService();

bool isAuthenticated = await authService.Authenticate(username, password, schoolId);

if (isAuthenticated)
{
    var accessToken = await authService.GetAccessToken();
    // Use the accessToken to create an instance of ShowMyHomeworkClient
}
```

You must call the `Authenticate()` method with a valid `username`, `password` and `schoolId`. 

## Disclaimer
This library is not supported by ShowMyHomework.co.uk and is just the result of his author understanding about how the ShowMyHomework API works, 
from observing its interactions with the public website. There is no public documentation about the API and it can change any time
without notice, breaking this library. Therefore, it is not recommended to use this library in any business critical projects.