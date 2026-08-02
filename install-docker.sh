#!/usr/bin/env bash
set -euo pipefail

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== Подготовка: удаление симлинка docker.sock (если это Podman) ===${NC}"
# Удаляем симлинк, если он есть и указывает на podman — чтобы не мешал настоящему Docker
if [ -L /var/run/docker.sock ] && [[ "$(readlink -f /var/run/docker.sock)" == *"/run/podman/"* ]]; then
  echo "Обнаружен симлинк docker.sock -> podman. Удаляем..."
  sudo rm /var/run/docker.sock
else
  echo "Симлинк docker.sock либо отсутствует, либо не ведёт на podman. Пропускаем удаление."
fi

echo -e "${GREEN}=== Установка зависимостей для работы с HTTPS-репозиториями ===${NC}"
sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg lsb-release

echo -e "${GREEN}=== Добавление GPG-ключа Docker ===${NC}"
sudo mkdir -p /etc/apt/keyrings
# Скачиваем ключ
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo -e "${GREEN}=== Определение кодового имени Ubuntu (основа Mint) ===${NC}"
. /etc/os-release
UBUNTU_CODENAME="${UBUNTU_CODENAME:-$(echo "$VERSION_ID" | sed 's/\..*//')}"
# Если UBUNTU_CODENAME не определился, пробуем угадать по VERSION_ID
if [ -z "$UBUNTU_CODENAME" ]; then
  case "$VERSION_ID" in
    22*) UBUNTU_CODENAME="jammy" ;;
    21*) UBUNTU_CODENAME="focal" ;; # Mint 21 основан на Ubuntu 22.04, но иногда используют focal в старых инструкциях; лучше jammy
    20*) UBUNTU_CODENAME="focal" ;;
    *) echo "Не удалось автоматически определить кодовое имя Ubuntu. Проверьте /etc/os-release и задайте UBUNTU_CODENAME вручную."; exit 1 ;;
  esac
fi
echo "Кодовое имя Ubuntu: $UBUNTU_CODENAME"

echo -e "${GREEN}=== Добавление репозитория Docker ===${NC}"
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $UBUNTU_CODENAME stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

echo -e "${GREEN}=== Обновление списка пакетов ===${NC}"
sudo apt-get update

echo -e "${GREEN}=== Установка Docker Engine, CLI и Compose-плагина ===${NC}"
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

echo -e "${GREEN}=== Включение и запуск службы Docker ===${NC}"
sudo systemctl start docker
sudo systemctl enable docker

echo -e "${GREEN}=== Добавление текущего пользователя в группу docker ===${NC}"
# Добавляем пользователя в группу
sudo usermod -aG docker "$USER"

echo ""
echo -e "${RED}=== ВАЖНО: изменения группы вступят в силу только после нового сеанса! ===${NC}"
echo "1. Закройте терминал и откройте новый, ИЛИ"
echo "2. Выйдите из системы и войдите снова."
echo ""
echo "После этого выполните:"
echo "  groups                # должна быть группа docker"
echo "  docker info           # должен показать информацию о демоне"
echo "  docker compose version"
echo ""
echo "Если docker info всё ещё выдаёт permission denied — обязательно перелогиньтесь."

