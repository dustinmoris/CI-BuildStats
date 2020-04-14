# [BuildStats.info](https://buildstats.info)
A little SVG widget to display build history charts and other badges for public repositories.

[![Build status](https://ci.appveyor.com/api/projects/status/7g3sppml9ewumr9i/branch/master?svg=true)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/branch/develop)

[![Build History](https://buildstats.info/appveyor/chart/dustinmoris/CI-BuildStats?branch=develop)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history?branch=develop)

## Table of contents

- [Build History Chart](#build-history-chart)
    - [How it works](#how-it-works)
    - [Configuration](#configuration)
- [NuGet Badges](#nuget-badges)
- [MyGet Badges](#myget-badges)
- [API Documentation](#api-documentation)
- [Docker image](#docker-image)
- [Contributing](#contributing)
- [Support](#support)

## Build History Chart

The SVG widget currently works for public repositories built with:

<a href="https://www.appveyor.com/" title="AppVeyor"><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/master/assets/AppVeyor.png" width="80" height="80" style="margin-right: 30px;" alt="AppVeyor" title="AppVeyor"/></a><a href="https://travis-ci.org/" title="TravisCI"><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/master/assets/TravisCI.jpg" width="80" height="80" style="margin-right: 30px;" alt="TravisCI" title="TravisCI"/></a><a href="https://circleci.com/" title="CircleCI"><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/master/assets/CircleCI.png" width="80" height="80" style="margin-right: 10px;" alt="CircleCI" title="CircleCI"/></a><a href="https://dev.azure.com/" title="Azure Pipelines"><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/master/assets/AzurePipelines.png" width="80" height="80" style="margin-right: 30px;" alt="Azure Pipelines" title="Azure Pipelines"/></a>

### How it works

The base URL to the SVG widget is:

```
https://buildstats.info/{buildSystem}/chart/{account}/{project}[/{definitionId}]
```

Replace `{buildSystem}` with one of the supported build systems:

-   appveyor
-   travisci
-   circleci
-   azurepipelines

Replace `{account}` and `{project}` with your personal values.

For example `https://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats` displays the build history chart for this particular project.

The complete markdown for the above chart is as following:

```
[![Build history](https://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history)
```

The URL for an Azure Pipelines powered graph has an additional route argument for the definition ID which is an Azure Pipelines specific concept:

```
https://buildstats.info/azurepipelines/chart/MyAccount/MyProject/12
```

### Configuration

#### Filtering for a specific branch

By default the widget will render a chart for builds from all branches.

You can select a specific branch by appending the `branch` parameter to the URL (optional):

```
https://buildstats.info/{buildSystem}/chart/{account}/{project}?branch={branch}
```

#### Changing the number of builds

You can specify the maximum build count by appending the `buildCount` parameter to the URL (optional):

```
https://buildstats.info/{buildSystem}/chart/{account}/{project}?buildCount={number}
```

#### Excluding builds from a pull request

Use the `includeBuildsFromPullRequest` parameter to include or exclude builds from a pull request (optional):

```
https://buildstats.info/{buildSystem}/chart/{account}/{project}?includeBuildsFromPullRequest={true/false}
```

#### Hiding the text

You can hide the build stats by appending the `showStats` parameter to the URL (optional):
```
https://buildstats.info/{buildSystem}/chart/{account}/{project}?showStats={true/false}
```

#### Full URL

The full URL to the SVG widget is:

```
https://buildstats.info/{buildSystem}/chart/{account}/{project}[?buildCount={buildCount}&branch={branch}&includeBuildsFromPullRequest={includeBuildsFromPullRequest}&showStats={true/false}]
```

## NuGet Badges

The URL to the NuGet badge is:

```
https://buildstats.info/nuget/{packageName}
```

For example the badge and Markdown for the [NUnit](https://github.com/nunit/nunit) NuGet badge would be:

[![NuGet Badge](https://buildstats.info/nuget/nunit)](https://www.nuget.org/packages/NUnit/)

```
[![NuGet Badge](https://buildstats.info/nuget/nunit)](https://www.nuget.org/packages/NUnit/)
```

### Including PreRelease packages

You can append the `includePreReleases=true` flag to include pre-release packages:

[![NuGet Badge](https://buildstats.info/nuget/NServiceBus.PostgreSQL?includePreReleases=true)](https://www.nuget.org/packages/NServiceBus.PostgreSQL/1.0.0-CI00021)

```
[![NuGet Badge](https://buildstats.info/nuget/NServiceBus.PostgreSQL?includePreReleases=true)](https://www.nuget.org/packages/NServiceBus.PostgreSQL/1.0.0-CI00021)
```

### Setting a specific package version

By adding the `packageVersion` query parameter you can set a specific version:

[![NuGet Badge](https://buildstats.info/nuget/Giraffe?packageVersion=3.0.0)](https://www.nuget.org/packages/Giraffe/3.0.0)

```
[![NuGet Badge](https://buildstats.info/nuget/NServiceBus.PostgreSQL?includePreReleases=true)](https://www.nuget.org/packages/NServiceBus.PostgreSQL/1.0.0-CI00021)
```

### Setting a fixed width

If you want to control the width of the version and/or downloads label then you can use the `vWidth` and `dWidth` query parameters. Both accept an integer value representing the width in pixels:

[![NuGet Badge](https://buildstats.info/nuget/Giraffe?vWidth=100&dWidth=100)](https://www.nuget.org/packages/Giraffe)

```
[![NuGet Badge](https://buildstats.info/nuget/Giraffe?vWidth=100&dWidth=100)](https://www.nuget.org/packages/Giraffe)
```

## MyGet Badges

MyGet badges are supported for both the standard MyGet feed as well as MyGet Enterprise customers.

The URL to a MyGet badge from the official feed is:

```
https://buildstats.info/myget/{feedName}/{packageName}
```

For example the badge and Markdown for the [NEventSocket](https://github.com/danbarua/NEventSocket) MyGet badge would be:

[![MyGet Badge](https://buildstats.info/myget/neventsocket-prerelease/NEventSocket)](https://www.myget.org/feed/neventsocket-prerelease/package/nuget/NEventSocket)

```
[![MyGet Badge](https://buildstats.info/myget/neventsocket-prerelease/NEventSocket)](https://www.myget.org/feed/neventsocket-prerelease/package/nuget/NEventSocket)
```

The URL to a MyGet badge from an Enterprise feed is:

```
https://buildstats.info/myget/{subDomain}/{feedName}/{packageName}
```

For example the badge and Markdown for the [Microsoft.Bot.Builder](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder) MyGet badge would be:

[![MyGet Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder?includePreReleases=true)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder)

```
[![MyGet Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder?includePreReleases=true)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder)
```

### Additional settings

The `includePreReleases`, `packageVersion`, `vWidth` and `dWidth` query parameters work the same way as for NuGet badges (see above for more information).

## API Documentation

For complete API documentation please check out the attached [api.raml](https://github.com/dustinmoris/CI-BuildStats/blob/master/api.raml) or visit the public [API Portal](https://anypoint.mulesoft.com/apiplatform/dustinmoris/#/portals/organizations/1c966d9b-793c-46bc-a87a-427b9a4a9b4a/apis/76973/versions/79960).

## Docker image

You can also self host the application by running [CI-BuildStats from a Docker container](https://hub.docker.com/r/dustinmoris/ci-buildstats/).

## Contributing

Feedback is welcome and pull requests get accepted.

## Support

If you've got value from any of the content which I have created, but pull requests are not your thing, then I would also very much appreciate your support by buying me a coffee.

<a href="https://www.buymeacoffee.com/dustinmoris" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>