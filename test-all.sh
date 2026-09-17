#!/usr/bin/env bash
while true; do
    dotnet test
    rc=$?
    if [ "$rc" -ne 0 ]; then
        exit $rc
    fi
    sleep 1
done
