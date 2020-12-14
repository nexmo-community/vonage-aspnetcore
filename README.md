# Vonage ASP.NET Core

[![NuGet](http://img.shields.io/nuget/v/Vonage.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/Vonage.AspNetCore/)

<img src="https://developer.nexmo.com/assets/images/Vonage_Nexmo.svg" height="48px" alt="Nexmo is now known as Vonage" />

The Vonage ASP.NET Core project is a middleware utility library meant to help you integrate Vonage more easily into your workflows. It provides three primary functionalities:

1. Direct, configurable Dependency Injection of the Vonage Client into all your services
2. Controller-less handling of all Vonage Webhooks
3. Direct verification of all inbound webhooks using a [signing secret](https://developer.nexmo.com/concepts/guides/signing-messages#validate-the-signature-on-incoming-messages)

## Welcome to Vonage

<!-- change "github-repo" at the end of the link to be the name of your repo, this helps us understand which projects are driving signups so we can do more stuff that developers love -->

If you're new to Vonage, you can [sign up for a Vonage API account](https://dashboard.nexmo.com/sign-up?utm_source=DEV_REL&utm_medium=github&utm_campaign=github-repo) and get some free credit to get you started.

## Usage

Include this project in yours by installing the `Vonage.AspNetCore` nuget package:

`dotnet add package Vonage.AspNetCore`

## Dependency injection

There are two methods that you can use to dependency inject the VonageClient into all your services. Via Configuration, or via credentials.

### Using Configuration

You can use your apps `IConfiguration` object to configure the VonageClient injected into your services. To do this simply pass in the configuration object. In your `Startup.cs` file's `ConfigureServices` method, run the following:

```csharp
services.AddVonageService(Configuration);
```

This expects a set of credentials to live a `Vonage` object inside your `appsettings.json` file e.g.:

```json
"Vonage": {
    "API_SECRET": "API_SECRET",
    "API_KEY": "API_KEY"
  }
```

The expected fields are as follows:

Key | Description
----|--------------
Vonage:API_KEY | Your Vonage API Key, Should be paired with either `Vonage:API_SECRET` or `Vonage:SIGNATURE_SECRET` and `Vonage:SIGNATURE_METHOD`
Vonage:API_SECRET | Your Vonage Account's API Secret, should be paired with `Vonage:API_KEY`
Vonage:Private_Key_Path | A path to your application's private key, must be paired with `Vonage:APPLICATION_ID`
Vonage:APPLICATION_ID | A [Vonage Application](https://developer.nexmo.com/application/overview) Id - must be paired with `Vonage:Private_Key_Path`
Vonage:SIGNATURE_SECRET | The Signature Secret for you Vonage Account
Vonage:SIGNATURE_METHOD | The method your account is set to for signing SMS messages, options are `md5hash`, `md5`, `sha1`, `sha256`, and `sha512`

### Using A Vonage Credentials Object

Alternatively you can create a  `Vonage.Request.Credentials` Object and and simply pass it into the `AddVonageService` request. again in the `ConfigureServices` method of your `Startup.cs` file you can add this like:

```csharp
var creds = Credentials.FromApiKeyAndSecret(apiKey, apiSecret);
services.AddVonageService(creds)
```

## Handling Inbound Webhooks

This project also simplifies the handling of inbound webhooks from the Vonage APIs. After you've set up your webhooks you can now easily handle them by telling the Vonage Middleware what type your trying to handle along with the route you expect them at (The URL that you've set without your sitebase). So for example if you wanted to do an Async Numbers Insight request and handle it via middleware you can now, inside of the `Configure` method of your `Startup.cs` file use the following:

```csharp
app.UseWebhookRoute<AdvancedInsightsResponse>(
    niResponse => Console.WriteLine($"Insights Retrieved for {niResponse.InternationalFormatNumber}"),
    "/webhooks/ni");
```

This will only apply to requests received at the specified route (in the above case `/webhooks/ni`), consequentially it will terminate the request immediately after the middleware executes. If you would like the request to preceded onto the next piece of middleware in your pipeline, you can pass in `true` for the `invokeNextMiddleware` parameter in the `UseWebhookRoute` request

## Validating inbound SMS

This project also enables you to validate inbound SMS messages using the signatures provided by the [Vonage SMS Signing capability](https://developer.nexmo.com/concepts/guides/signing-messages). Now rather than having to setup a route and validate an inbound SMS, all you need to do is to add a `app.UseVonageSms` call in your `Configure` method in your `Startup.cs` file. You just need to pass in your Signature Secret and the method you're using for message signatures.

```csharp
app.UseVonageSms(
    (sms) => Console.WriteLine($"This is the message from a validated SMS: {sms.Text}"),
    path: "/webhooks/inbound-sms",
    signatureSecret: "SIGNATURE_SECRET");
```

## Getting Help

We love to hear from you so if you have questions, comments or find a bug in the project, let us know! You can either:

* Open an issue on this repository
* Tweet at us! We're [@VonageDev on Twitter](https://twitter.com/VonageDev)
* Or [join the Vonage Developer Community Slack](https://developer.nexmo.com/community/slack)

## Further Reading

* Check out the Developer Documentation at <https://developer.nexmo.com>

<!-- add links to the api reference, other documentation, related blog posts, whatever someone who has read this far might find interesting :) -->

