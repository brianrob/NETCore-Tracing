#!/bin/bash

echo "Generating deps file from $1."
sed -i -e 's/NETCore\.Tracing\.Service\.dll/lib\/netcoreapp2\.0\/NETCore\.Tracing\.Service\.dll/g' $1
sed -i -e 's/NETCore\.Tracing\.Service\.ASPNET\.dll/lib\/netcoreapp2\.0\/NETCore\.Tracing\.Service\.ASPNET\.dll/g' $1
