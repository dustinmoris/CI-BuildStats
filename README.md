# CI-BuildStats
A little SVG badge to display an AppVeyor or TravisCI build history chart.

[![Build status](https://ci.appveyor.com/api/projects/status/dchv355fwpsy85xb?svg=true)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats)

[![Build history](http://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history)

## How to use it

The URL to the SVG badge is:
```
http://buildstats.info/{buildSystem}/chart/{account}/{project}[?buildCount={buildCount}&branch={branch}&includeBuildsFromPullRequest={includeBuildsFromPullRequest}&showStats={true/false}]
```

Replace {buildSystem} with one of the supported build systems:
- appveyor
- travisci

Replace {account} and {project} with your personal values.

For example http://buildstats.info/appveyor/chart/dustinmoris/ci-buildstats will display the build history chart for this project.

### Adding the SVG badge to your GitHub README file

Use this snippet to add a badge to your README:

```
[![Build history](http://buildstats.info/{buildSystem}/chart/{account}/{project})]({urlToYourBuildHistory})
```
The first URL in this snippet links to the SVG badge and {urlToYourBuildHistory} links to the project's actual build history page.

For AppVeyor builds it is in the format of
```
https://ci.appveyor.com/project/{account}/{project}/history
```

For TravisCI builds it is in the format of
```
https://travis-ci.org/{account}/{project}/builds
```

### Configuration

#### Filtering for a specific branch

By default the widget will draw a chart for builds from all branches.

You can select a specific branch by appending the branch parameter to the URL (optional):
```
http://buildstats.info/{buildSystem}/chart/{account}/{project}?branch={branch}
```

#### Changing the number of builds

You can specify the maximum build count by appending the buildCount parameter to the URL (optional):
```
http://buildstats.info/{buildSystem}/chart/{account}/{project}?buildCount={number}
```

#### Excluding builds from a pull request

Use the includeBuildsFromPullRequest parameter to include or exclude builds from a pull request:
```
http://buildstats.info/{buildSystem}/chart/{account}/{project}?includeBuildsFromPullRequest={true/false}
```

##### Example
Showing 15 builds in the badge:

[![Build history](http://buildstats.info/appveyor/chart/dustinmoris/dustedcodes?buildCount=15)](https://ci.appveyor.com/project/dustinmoris/dustedcodes/history)

#### Hiding the text

You can hide the build stats by appending the showstats parameter to the URL (optional):
```
http://buildstats.info/{buildSystem}/chart/{account}/{project}?showstats=false
```

##### Example
Hiding the build stats:

[![Build history](http://buildstats.info/appveyor/chart/dustinmoris/dustedcodes?showstats=false)](https://ci.appveyor.com/project/dustinmoris/dustedcodes/history)

## Support

Currently this works for AppVeyor and TravisCI builds.

Feedback is much appreciated and pull requests get accepted.
