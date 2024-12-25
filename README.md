![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Thundermaker300/RoSharp/build.yml?style=for-the-badge)
![GitHub Issues](https://img.shields.io/github/issues/Thundermaker300/RoSharp?style=for-the-badge)
![GitHub code size](https://img.shields.io/github/languages/code-size/Thundermaker300/RoSharp?style=for-the-badge)

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

## Credits
* [Robloxdotnet](https://github.com/Loravis/Robloxdotnet) by [Loravis](https://github.com/Loravis) -- Original inspiration for RoSharp, various inspirations taken from this project.
* [RoSeal](https://www.roseal.live/) -- Providing the API/Database that is used for `Experience.GetCommunityWikiUrlAsync`.

## Donations
RoSharp doesn't cost anything but donations are greatly appreciated! [Click here to donate](https://ko-fi.com/P5P416152H).