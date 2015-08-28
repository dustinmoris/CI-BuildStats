# CI-BuildStats
A little SVG badge to display AppVeyor build statistics.

[![Build status](https://ci.appveyor.com/api/projects/status/dchv355fwpsy85xb?svg=true)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats)

[![Build history](http://ci-buildstats.azurewebsites.net/appveyor/chart/dustinmoris/ci-buildstats)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history)

## How to use it

The URL to the SVG badge is:
```
https://ci-buildstats.azurewebsites.net/appveyor/chart/{account}/{project}
```

You have to replace {account} and {project} with your personal values.

For example https://ci-buildstats.azurewebsites.net/appveyor/chart/dustinmoris/ci-buildstats will display the build history badge for this project.

### Adding the SVG badge to your GitHub README file

Use this snippet to add a badge to your README:

```
[![Build history](http://ci-buildstats.azurewebsites.net/appveyor/chart/{account}/{project})](https://ci.appveyor.com/project/{account}/{project}/history)
```
The first URL in this snippet links to the SVG badge and the second URL links to the project's actual build history page in AppVeyor.

You have to replace {account} and {project} with your personal values.

### Configuration

#### Changing the number of builds

You can specify the maximum build count by appending the buildCount parameter to the URL (optional):
```
https://ci-buildstats.azurewebsites.net/appveyor/chart/{account}/{project}?buildCount={number}
```

##### Example
Showing 15 builds in the badge:

[![Build history](http://ci-buildstats.azurewebsites.net/appveyor/chart/dustinmoris/dustedcodes?buildCount=15)](https://ci.appveyor.com/project/dustinmoris/dustedcodes/history)

#### Hiding the text

You can hide the build stats by appending the showstats parameter to the URL (optional):
```
https://ci-buildstats.azurewebsites.net/appveyor/chart/{account}/{project}?showstats=false
```

##### Example
Hiding the build stats:

[![Build history](http://ci-buildstats.azurewebsites.net/appveyor/chart/dustinmoris/dustedcodes?showstats=false)](https://ci.appveyor.com/project/dustinmoris/dustedcodes/history)

## Support

At the moment this only works for AppVeyor builds, but soon I will be adding support for Travis-CI as well.
