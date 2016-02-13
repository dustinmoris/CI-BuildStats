# Buildstats.info
A little SVG widget to display build history charts for public repositories.

[![Build status](https://ci.appveyor.com/api/projects/status/dchv355fwpsy85xb?svg=true)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats)

[![Build history](http://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history)

## Support

The SVG widget currently works for public repositories built with:

<a href="https://www.appveyor.com/" title="AppVeyor">AppVeyor<br/><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/feature/Add_support_for_CircleCI/BuildStats.Web/Assets/appveyor.png" width="80px" height="80px" alt="AppVeyor" title="AppVeyor"/></a>
<a href="https://travis-ci.org/" title="TravisCI">TravisCI<br/><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/feature/Add_support_for_CircleCI/BuildStats.Web/Assets/travisci.jpeg" width="80px" height="80px" alt="TravisCI" title="TravisCI"/></a>
<a href="https://circleci.com/" title="CircleCI">CircleCI<br/><img src="https://raw.githubusercontent.com/dustinmoris/CI-BuildStats/feature/Add_support_for_CircleCI/BuildStats.Web/Assets/circleci.png" width="80px" height="80px" alt="CircleCI" title="CircleCI"/></a>

## How it works

The base URL to the SVG widget is:

```
http://buildstats.info/{buildSystem}/chart/{account}/{project}
```

Replace `{buildSystem}` with one of the supported build systems:

-   appveyor
-   travisci
-   circleci

Replace `{account}` and `{project}` with your personal values.

For example `http://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats` displays the build history chart for this particular project.

The complete markdown for the above chart is as following:

```
[![Build history](http://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history)
```

## Configuration

#### Filtering for a specific branch

By default the widget will render a chart for builds from all branches.

You can select a specific branch by appending the `branch` parameter to the URL (optional):

```
http://buildstats.info/{buildSystem}/chart/{account}/{project}?branch={branch}
```

#### Changing the number of builds

You can specify the maximum build count by appending the `buildCount` parameter to the URL (optional):

```
http://buildstats.info/{buildSystem}/chart/{account}/{project}?buildCount={number}
```

#### Excluding builds from a pull request

Use the `includeBuildsFromPullRequest` parameter to include or exclude builds from a pull request (optional):

```
http://buildstats.info/{buildSystem}/chart/{account}/{project}?includeBuildsFromPullRequest={true/false}
```

#### Hiding the text

You can hide the build stats by appending the `showstats` parameter to the URL (optional):
```
http://buildstats.info/{buildSystem}/chart/{account}/{project}?showstats={true/false}
```

#### Full URL

The full URL to the SVG widget is:

```
http://buildstats.info/{buildSystem}/chart/{account}/{project}[?buildCount={buildCount}&branch={branch}&includeBuildsFromPullRequest={includeBuildsFromPullRequest}&showStats={true/false}]
```

## Examples

The SVG widget for [Buildstats.info](http://buildstats.info)

-   which is build with AppVeyor
-   for the last 40 builds
-   and only from the master branch

is as following:

[![Build history](http://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats?branch=master&buildCount=40)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history?branch=master)

## Contribution

Feedback is welcome and pull requests get accepted.
