#!/bin/sh

dotnet tool restore

dotnet csharpier format .
dotnet xstyler -r -d src -c "src/Settings.XamlStyler"