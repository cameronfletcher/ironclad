#!/usr/bin/env bash

dotnet run --project ./build/build.csproj -- --parallel "$@"