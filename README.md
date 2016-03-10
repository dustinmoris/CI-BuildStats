# [Buildstats.info](https://buildstats.info)
A little SVG widget to display build history charts and other badges for public repositories.

[![Build status](https://ci.appveyor.com/api/projects/status/dchv355fwpsy85xb?svg=true)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats)

[![Build history](https://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history)

## Build History Chart

### Support

The SVG widget currently works for public repositories built with:

<a href="https://www.appveyor.com/" title="AppVeyor"><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/master/BuildStats.Web/Assets/appveyor.png" width="80" height="80" style="margin-right: 10px;" alt="AppVeyor" title="AppVeyor"/></a><a href="https://travis-ci.org/" title="TravisCI"><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/master/BuildStats.Web/Assets/travisci.jpeg" width="80" height="80" style="margin-right: 10px;" alt="TravisCI" title="TravisCI"/></a><a href="https://circleci.com/" title="CircleCI"><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/master/BuildStats.Web/Assets/circleci.png" width="80" height="80" style="margin-right: 10px;" alt="CircleCI" title="CircleCI"/></a>

### How it works

The base URL to the SVG widget is:

```
https://buildstats.info/{buildSystem}/chart/{account}/{project}
```

Replace `{buildSystem}` with one of the supported build systems:

-   appveyor
-   travisci
-   circleci

Replace `{account}` and `{project}` with your personal values.

For example `https://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats` displays the build history chart for this particular project.

The complete markdown for the above chart is as following:

```
[![Build history](https://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history)
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

You can hide the build stats by appending the `showstats` parameter to the URL (optional):
```
https://buildstats.info/{buildSystem}/chart/{account}/{project}?showstats={true/false}
```

#### Full URL

The full URL to the SVG widget is:

```
https://buildstats.info/{buildSystem}/chart/{account}/{project}[?buildCount={buildCount}&branch={branch}&includeBuildsFromPullRequest={includeBuildsFromPullRequest}&showStats={true/false}]
```

### Examples

The SVG widget for [Buildstats.info](https://buildstats.info)

-   which is build with AppVeyor
-   for the last 40 builds
-   and only from the master branch

is as following:

[![Build history](https://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats?branch=master&buildCount=40)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history?branch=master)

## NuGet Badge

NuGet badges are in beta state at the moment.

The URL to the NuGet badge is:

```
https://buildstats.info/nuget/{packageName}
```

For example the badge and Markdown for the [NUnit](https://github.com/nunit/nunit) NuGet badge would be:

[![NuGet Badge](https://buildstats.info/nuget/nunit)](https://www.nuget.org/packages/NUnit/)

```
[![NuGet Badge](https://buildstats.info/nuget/nunit)](https://www.nuget.org/packages/NUnit/)
```

## MyGet Badge

MyGet badges are in alpha state.

The URL to the MyGet badge is:

```
https://buildstats.info/myget/{feedName}/{packageName}
```

For example the badge and Markdown for the [NEventSocket](https://github.com/danbarua/NEventSocket) MyGet badge would be:

[![MyGet Badge](https://buildstats.info/myget/neventsocket-prerelease/NEventSocket)](https://www.myget.org/feed/neventsocket-prerelease/package/nuget/NEventSocket)

```
[![MyGet Badge](https://buildstats.info/myget/neventsocket-prerelease/NEventSocket)](https://www.myget.org/feed/neventsocket-prerelease/package/nuget/NEventSocket)
```

## Contribution

Feedback is welcome and pull requests get accepted.
