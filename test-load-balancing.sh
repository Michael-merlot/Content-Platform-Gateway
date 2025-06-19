#!/bin/bash
echo "Тестирование балансировки нагрузки API Gateway..."

echo "1. Тест стратегии Round Robin (запросы распределяются между серверами)"
for i in {1..10}; do
    curl -s http://localhost/api/Diagnostics/redis-test | jq -r .
    echo "-----"
    sleep 1
done

echo "2. Тест привязки сессий (запросы должны идти на один сервер)"
curl -s -c cookies.txt http://localhost/user/api/Diagnostics/session-test?value=sticky-test | jq -r .
echo "-----"

for i in {1..5}; do
    curl -s -b cookies.txt http://localhost/user/api/Diagnostics/session-test | jq -r .
    echo "-----"
    sleep 1
done

echo "3. Тест отказоустойчивости (остановка одного сервера)"
echo "Останавливаем api-gateway-1..."
docker stop $(docker ps -q -f name=api-gateway-1)

echo "Проверяем, что система продолжает работать..."
for i in {1..5}; do
    curl -s http://localhost/api/Diagnostics/redis-test | jq -r .
    echo "-----"
    sleep 1
done

echo "Восстанавливаем api-gateway-1..."
docker start $(docker ps -aq -f name=api-gateway-1)

echo "Тестирование завершено!"
