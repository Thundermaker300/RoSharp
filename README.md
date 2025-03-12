[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Thundermaker300/RoSharp/build.yml?style=for-the-badge)](https://github.com/Thundermaker300/RoSharp/actions)
[![GitHub Issues](https://img.shields.io/github/issues/Thundermaker300/RoSharp?style=for-the-badge)](https://github.com/Thundermaker300/RoSharp/issues)
[![GitHub code size](https://img.shields.io/github/languages/code-size/Thundermaker300/RoSharp?style=for-the-badge)](https://github.com/Thundermaker300/RoSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/RoSharp?style=for-the-badge&label=NuGet%20Downloads)](https://www.nuget.org/packages/RoSharp)

[![Discord](https://img.shields.io/discord/1305657567137107978?color=738adb&label=Discord&logo=discord&logoColor=white&style=for-the-badge)](https://discord.gg/3hH7qT33Wy)

# RoSharp
RoSharp is a C#/.NET utility package designed as a wrapper for Roblox's Web API system. The framework is built on .NET 8.0, and requires version 13.0.3 or greater of Newtonsoft.Json (which will be installed alongside the framework if installed with NuGet).

See [wiki](https://github.com/Thundermaker300/RoSharp/wiki) for all extensive documentation. This wiki is work in progress! Every public member within the framework is documented via C#'s XML documentation, so users of Visual Studio and Visual Studio Code (and likely other IDEs) should be covered!

Join our [Discord](https://discord.gg/3hH7qT33Wy) if you have questions!

## Installation
RoSharp can be installed [directly from NuGet](https://nuget.org/packages/RoSharp) through your IDE's package manager or with the following command in the command-line.

```
Install-Package RoSharp -Version <version>
```
RoSharp can also be installed by downloading the DLL under the "Releases" and adding it to your project manually.

## Features
Below are some of the following features that are available in RoSharp:
* Users: Get information of any user on the platform, including their badges, inventory (if visible), experiences, and more.
* Communities: Manage your community from a console app, Discord bot, etc! This framework includes the ability to make new ranks, accept and decline join requests, delete wall posts, remove members, and more!
* Assets: View data of assets and manage owned assets directly from the framework.
* Experiences: See all sorts of data from your experiences, even MAU and income data, with much more to come! You can also modify experiences, ban users, etc. You can even modify in-experience DataStores and send MessagingService messages using this framework thanks to Roblox's open-cloud API. The first of its kind to have these game-changing capabilities!
* Price Floors: Integrated API to view Roblox's live price floor data. Use this data alongside the Assets API to calculate the price of an item automatically, and keep track of ever-changing Roblox price floors.
* Custom Requests: RoSharp provides an easy way for you, the user, to make your own requests to the Roblox API using the framework's authentication system. Gone are the days of digging into the depths of authentication yourself, this framework has you covered! Get the URL and go, while the framework does the HTTP magic of headers and content itself.
* (BONUS) DevForum API: Read-only API for viewing the DevForum from the eyes of a non-authenticated user via Discourse's APIs. See posts and replies of public posts, including official Roblox updates.
* Much more!

## Samples
For all samples and documentation see the [wiki](https://github.com/Thundermaker300/RoSharp/wiki).
### Login with .ROBLOSECURITY
```cs
using RoSharp;

string code = ".ROBLOSECURITY_CODE_HERE";

Session session = new Session();
await session.LoginAsync(code); // Awaited as we sign into Roblox.

// If successful, the session is now logged in and can be used for API that requires authentication.
Console.WriteLine(session.AuthUser.Username); // Print the name of the user that is authenticated.

// Reminder: You should NEVER share your .ROBLOSECURITY authentication token in any public code!
```

### Login with API Key
```cs
using RoSharp;

string apiKey = "API_KEY_HERE";

Session session = new Session();
session.SetAPIKey(apiKey); // Does not need awaited as there's no async operation, it just sets the API key internally.
```

### Get User Information
```cs
using RoSharp;
using RoSharp.API;

// Authentication is optional for this example and the session does not need to be provided. However, not all information is available to an unauthorized viewer.

User user = await User.FromId(USER_ID_HERE, SESSION_HERE);
// OR
User user = await User.FromUsername(USERNAME_HERE, SESSION_HERE);

Console.WriteLine(user.Username);
```

### Roblox Communities
```cs
using RoSharp;
using RoSharp.API;
using RoSharp.API.Communities;

// Get Community information
Community community = await Community.FromId(COMMUNITY_ID_HERE, SESSION_HERE);
Console.WriteLine(community.Name);

// Get the member manager for the below examples
MemberManager members = await community.GetMemberManagerAsync();

// Get user's role in a community
Role? userRole = await members.GetRoleInCommunityAsync(USER_HERE); // Object can be substituted for ID and/or username.
if (userRole != null)
{
    Console.WriteLine(userRole.Name);
}

// Set user's role in community
await members.SetRankAsync(USER_HERE, ROLE_OBJECT_HERE); // Objects can be substituted for UserId/Username and Role name/ID.
```

## Credits
* [Robloxdotnet](https://github.com/Loravis/Robloxdotnet) by [Loravis](https://github.com/Loravis) -- Original inspiration for RoSharp, various inspirations taken from this project.
* [RoSeal](https://www.roseal.live/) -- Providing the API/Database that is used for `Experience.GetCommunityWikiUrlAsync`.

## Donations
RoSharp doesn't cost anything but donations are greatly appreciated! [Click here to donate](https://ko-fi.com/P5P416152H).
