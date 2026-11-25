#!/usr/bin/env bash

if [[ $# -lt 2 ]]; then
  echo "Использование: $0 <сервис> <миграция>"
  exit 1
fi

service="$1"
migration="$2"

dotnet ef migrations add "$migration" -p "$service"/"$service".Database -s "$service"/"$service"
