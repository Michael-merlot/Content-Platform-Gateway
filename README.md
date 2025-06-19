# Архитектура включает:
- 3 экземпляра API Gateway в контейнерах Docker
- NGINX в качестве балансировщика нагрузки
- Redis для распределенного хранения состояния
- Механизмы проверки работоспособности и отказоустойчивости

[Клиенты] -> [Nginx балансировщик] -> [Api gateway] -> [Redis]

## Клонировать репозиторий и войти в директорию 
- cd content-platform-gateway

## Запуск контейнеров
- docker-compose up -d

## Проверка статуса
- docker-compose ps

# API для тестирования
## 1. Проверка работы Redis и балансировки
- GET http://localhost/api/Diagnostics/redis-test

Пример ответа:

{

  "success": true,
  
  "keyExists": true,
  
  "originalValue": "Redis test at 06/19/2025 06:31:47",
  
  "retrievedValue": "Redis test at 06/19/2025 06:31:47",
  
  "usingRedis": true
  
}

## 2. Проверка сессий
### Установка значения в сессию
- GET http://localhost/api/Diagnostics/session-test?value=test-value

### Получение значения сессии
- GET http://localhost/api/Diagnostics/session-test

## 3. Проверка здоровья системы
- GET http://localhost/health
### Тестирование отказоустойчивости
#### Остановка одного экземпляра API Gateway
- docker stop content-platform-gatewaytest-api-gateway-1-1

#### Проверка, что система продолжает работать
- curl http://localhost/api/Diagnostics/redis-test

#### Запуск остановленного экземпляра
- docker start content-platform-gatewaytest-api-gateway-1-1
