#!/usr/bin/env bash
dotnet restore
dotnet build
dotnet test ./test/Axe.SimpleHttpMock.Test/Axe.SimpleHttpMock.Test.csproj
