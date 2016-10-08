#!/bin/bash

# Stop the script if a command returns a non 0 exit code
set -ev

# Check if the variables exist otherwise read from shell arguments
TRAVIS_TAG=${TRAVIS_TAG:-$1}
DOCKER_USERNAME=${DOCKER_USERNAME:-$2}
DOCKER_PASSWORD=${DOCKER_PASSWORD:-$3}

# ---------------------------------
# BUILD
# ---------------------------------

nuget restore BuildStats.sln
xbuild /p:Configuration=Release BuildStats.sln

# ---------------------------------
# DEPLOY
# ---------------------------------

# Only publish to Docker Hub on tags
if [ -n "$TRAVIS_TAG" ]; then

    # Read the tag name into an array split by the dot (.)
    IFS='.' read -r -a tag <<< "$TRAVIS_TAG"

    # Remove a leading v from the major version number (e.g. if the tag was v1.0.0)
    MAJOR="${tag[0]//v}"

    # Set the other parts of the sem ver to respective variables as well
    MINOR=${tag[1]}
    BUILD=${tag[2]}

    # Create a clean sem ver variable
    SEMVER="$MAJOR.$MINOR.$BUILD"

    # Build the Docker image
    docker build -t dustinmoris/ci-buildstats:$SEMVER ./BuildStats.Suave/bin/Release

    # Tag the same image with :latest as well
    docker tag dustinmoris/ci-buildstats:$SEMVER dustinmoris/ci-buildstats:latest

    # Login to Docker Hub and upload images
    docker login -u="$DOCKER_USERNAME" -p="$DOCKER_PASSWORD"
    docker push dustinmoris/ci-buildstats:$SEMVER
    docker push dustinmoris/ci-buildstats:latest
    
fi