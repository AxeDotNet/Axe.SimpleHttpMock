# Axe.SimpleHttpMock is a library to ease testing against external HTTP systems

## Latest status

[![Build Status](https://travis-ci.org/AxeDotNet/Axe.SimpleHttpMock.svg?branch=master)](https://travis-ci.org/AxeDotNet/Axe.SimpleHttpMock)

## Advantages

* No real socket lisetening server needed.
* You can simulate any base URI pattern for external systems.
* You can run test in parallel without problem.
* Simple syntax, various overloads. Creating test double for all external dependencies in a centralized or distributed forms.
* Cross platform. Supporting .NET Framework and .NET Core.

## Getting started

We assumed that you are using `HttpClient` to access 3rd party HTTP services (which is almost always the best choice), so you can do test doubling in the following lines of code.

```csharp
[Fact]
public async Task should_handle_request_to_http_service()
{
    var externalSystems = new MockHttpServer();
    externalSystems
        .WithService("http://www.base.com/user")
        .Api("account", "GET", HttpStatusCode.OK);

    HttpClient client = new HttpClient(externalSystems);
    HttpResponseMessage response = await client.GetAsync("http://www.base.com/user/account");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

You can define multiple routes for one service all in one place. And of course you can define endpoint at any time and at any place you want.

```csharp
externalSystems
    .WithSystem("http://www.base.com/user")
    .Api("account", "GET", HttpStatusCode.OK)
    .Api("account", "POST", HttpStatusCode.Created);
```

And you can define multiple HTTP methods in one method.

```csharp
externalSystems
    .WithSystem("http://www.base.com/user")
    .Api("account", new [] {"GET", "PUT"}, HttpStatusCode.OK);
```

URI template support. No problem, and you can get binded parameters as well.

```csharp
[Fact]
public async Task should_get_binding_parameters_of_uri()
{
    var externalSystems = new MockHttpServer();
    externalSystems
        .WithService("http://www.base.com")
        .Api(
            "user/{userId}/session/{sessionId}?p1={value1}&p2={value2}",
            "GET",
            bindedParameters => new { Parameter = bindedParameters }.AsResponse());

    HttpClient client = new HttpClient(externalSystems);

    HttpResponseMessage response = await client.GetAsync(
        "http://www.base.com/user/12/session/28000?p1=v1&p2=v2");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    string jsonString = await response.Content.ReadAsStringAsync();
    var content = JsonConvert.DeserializeAnonymousType(
        jsonString,
        new { Parameter = default(Dictionary<string, object>) });

    Assert.Equal("12", content.Parameter["userId"]);
    Assert.Equal("28000", content.Parameter["sessionId"]);
    Assert.Equal("v1", content.Parameter["value1"]);
    Assert.Equal("v2", content.Parameter["value2"]);
}
```

Want to get more information of the request? Of course you can. Just use the following overload:

```csharp
// The first parameter of the responseFunc is the original request object
public WithServiceClause Api(
    string uriTemplate,
    Func<HttpRequestMessage, IDictionary<string, object>, HttpResponseMessage> responseFunc,
    string name = null)
```

If no API definition is found, an NOT FOUND response will be returned by default. However, you can use `Default` method to define you own behavior.

```csharp
externalSystems
    .WithService("http://www.base.com")
    .Default(req => HttpStatusCode.Found.AsResponse());
```

Uri template is weak? You can use `RegexApi` method with regex syntax if you want.

Wants to verify mocking processes? You can get all calling history information for named API.

```csharp
[Fact]
public async Task should_verify_api_called_or_not_called()
{
    var externalSystems = new MockHttpServer();
    externalSystems
        .WithService("http://www.base.com")
        .Api("api1", "GET", HttpStatusCode.OK, "api1")
        .Api("api2", "GET", HttpStatusCode.OK, "api2");

    var client = new HttpClient(externalSystems);

    await client.GetAsync("http://www.base.com/api1");

    externalSystems["api1"].VerifyHasBeenCalled();
    externalSystems["api2"].VerifyNotCalled();
}
```

You can even get the cloned original request to verify all the things you passed are valid.

```csharp
[Fact]
public async void should_get_request_content_from_calling_history()
{
    var externalSystems = new MockHttpServer();
    externalSystems.WithService("http://www.base.com")
        .Api("login", "POST", HttpStatusCode.OK, "login");

    var client = new HttpClient(externalSystems);

    HttpResponseMessage response = await client.PostAsJsonAsync(
        "http://www.base.com/login",
        new { username = "n", password = "p" });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var actualRequestContent = await externalSystems["login"].SingleOrDefaultRequestContentAsync(
        new { username = string.Empty, password = string.Empty });

    Assert.Equal("n", actualRequestContent.username);
    Assert.Equal("p", actualRequestContent.password);
}
```

The indexer method for `MockHttpServer` actually returns a collection of calling history for every named API. So it can be easily extendable to verify much more.

Enjoy!

## Release Note

### 1.0.2

* Use local JSON serialization settings to avoid pollution.
* Mark unused methods as obsolete for IContentDeserializer.