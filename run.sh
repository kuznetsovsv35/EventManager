#!/usr/bin/env bash

if [[ $# -lt 1 ]]; then
  echo "Использование: $0 <сервис>"
  exit 1
fi

service="$1"

dotnet run --project "$service"/"$service"

