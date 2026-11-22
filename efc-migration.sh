#!/usr/bin/env bash

if [[ $# -lt 1 ]]; then
  echo "Использование: $0 <сервис>"
  exit 1
fi

service="$1"

dotnet ef migrations add InitialCreate -p "$service"/"$service".Database -s "$service"/"$service"
