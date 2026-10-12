# SagaDemo — Saga Pattern на ASP.NET Core

Учебный проект для практики **Saga Orchestration** в микросервисной архитектуре на **C# / ASP.NET Core**. Цель — изучить последовательность распределённых операций, компенсации при ошибках и идемпотентность без базы данных.

> **Статус:** реализована синхронная Saga через HTTP. RabbitMQ, MassTransit, Choreography и гибридная Saga — возможные следующие этапы, **пока не реализованы**.

## Архитектура

```text
Client
  |
  | POST /saga/orders (Idempotency-Key)
  v
SagaOrchestrator :5001
  |
  +-- 1. POST /orders ----------------> OrderService :5001
  |                                     Create / Cancel
  +-- 2. POST /payments --------------> PaymentService :5002
  |                                     Charge / Refund
  +-- 3. POST /inventory/reserve -----> InventoryService :5003
                                        Reserve / Release
```

При ошибке выполняются компенсации **успешных шагов в обратном порядке**:

```text
Order Created -> Payment Completed -> Inventory Failed
                                           |
                                           v
                                    Refund Payment
                                           |
                                           v
                                     Cancel Order
                                           |
                                           v
                                      Compensated
```

Оркестратор вызывает каждый сервис через HTTP; стрелки не означают прямые вызовы между бизнес-сервисами.

## Стек

- .NET 10 / ASP.NET Core Minimal API
- C# и `HttpClientFactory`
- In-Memory: `Dictionary` / `ConcurrentDictionary`
- **Без PostgreSQL, EF Core, Docker и брокера сообщений**

## Структура

```text
SagaDemo/
├── SagaDemo.slnx
├── .gitignore
├── README.md
└── src/
    ├── SagaOrchestrator/
    │   ├── Models/CreateOrderRequest.cs
    │   ├── Services/OrderSaga.cs
    │   ├── Services/SagaIdempotencyStore.cs
    │   └── Program.cs
    ├── OrderService/
    │   ├── Models/Order.cs
    │   ├── Services/OrderStore.cs
    │   └── Program.cs
    ├── PaymentService/
    │   ├── Models/Payment.cs
    │   ├── Services/PaymentStore.cs
    │   └── Program.cs
    └── InventoryService/
        ├── Models/Reservation.cs
        ├── Services/InventoryStore.cs
        └── Program.cs
```

## Запуск

Требуется установленный **.NET 10 SDK**. Из корневой директории:

```powershell
dotnet build
```

Запустите команды **в четырёх отдельных терминалах**:

```powershell
dotnet run --project src/SagaOrchestrator --launch-profile http
dotnet run --project src/OrderService --launch-profile http
dotnet run --project src/PaymentService --launch-profile http
dotnet run --project src/InventoryService --launch-profile http
```

| Компонент | Адрес |
| --- | --- |
| SagaOrchestrator | http://localhost:5000 |
| OrderService | http://localhost:5001 |
| PaymentService | http://localhost:5002 |
| InventoryService | http://localhost:5003 |

Проверка доступности: `GET /health` на каждом порту, если endpoint сохранён в `Program.cs` соответствующего сервиса.

## API

| Сервис | Метод | Маршрут | Назначение |
| --- | --- | --- | --- |
| SagaOrchestrator | POST | `/saga/orders` | Начать или повторно получить Saga |
| OrderService | POST | `/orders` | Создать заказ |
| OrderService | GET | `/orders/{id}` | Получить заказ |
| OrderService | POST | `/orders/{id}/cancel` | Отменить заказ |
| PaymentService | POST | `/payments` | Провести оплату |
| PaymentService | GET | `/payments/{orderId}` | Получить платёж |
| PaymentService | POST | `/payments/{orderId}/refund` | Вернуть оплату |
| InventoryService | POST | `/inventory/reserve` | Зарезервировать товар |
| InventoryService | GET | `/inventory/{orderId}` | Получить резерв |
| InventoryService | POST | `/inventory/{orderId}/release` | Освободить резерв |

## Проверка сценариев Saga

Отправляйте запросы в **SagaOrchestrator**, а не напрямую в сервисы. Для тестов можно использовать Postman, curl или REST Client.

### 1. Успешный заказ

```http
POST http://localhost:5000/saga/orders
Content-Type: application/json
Idempotency-Key: saga-success-001

{
  "amount": 150,
  "productId": "f8ac4d88-b69c-46fc-9984-c599ed30b649",
  "quantity": 2,
  "simulateInventoryFailure": false,
  "simulatePaymentFailure": false
}
```

Ожидается `200 OK` и `"status": "Completed"`; история включает `Order created`, `Payment completed`, `Inventory reserved`.

### 2. Ошибка InventoryService

Используйте **новый** ключ `saga-inventory-fail-001` и тело:

```json
{
  "amount": 150,
  "productId": "f8ac4d88-b69c-46fc-9984-c599ed30b649",
  "quantity": 2,
  "simulateInventoryFailure": true,
  "simulatePaymentFailure": false
}
```

Ожидается `409 Conflict`, `"status": "Compensated"`, а также шаги `Payment compensated` и `Order compensated` в истории. Заказ должен стать `Cancelled`, платёж — `Refunded`.

### 3. Ошибка PaymentService

Используйте новый ключ `saga-payment-fail-001`:

```json
{
  "amount": 150,
  "productId": "f8ac4d88-b69c-46fc-9984-c599ed30b649",
  "quantity": 2,
  "simulateInventoryFailure": false,
  "simulatePaymentFailure": true
}
```

Ожидается `409 Conflict`, `"status": "Compensated"`; выполняется только компенсация заказа, так как платёж не был проведён.

### 4. Идемпотентность всей Saga

Отправьте успешный запрос **несколько раз** с одинаковыми телом и `Idempotency-Key`. Ответ должен содержать **один и тот же `orderId`**, а шаги не должны выполняться заново.

Отправьте с **тем же ключом, но другой суммой** — ожидается `409 Conflict`. С **новым ключом** создаётся новая Saga и новый заказ.

### 5. Идемпотентность PaymentService

Напрямую отправьте в `POST http://localhost:5002/payments` одинаковый JSON несколько раз:

```json
{
  "orderId": "e1cfbbcb-b2b8-45ec-a493-56ff5e78f56b",
  "amount": 150
}
```

Первый запрос создаёт платёж (`201 Created`), следующие возвращают уже существующий (`200 OK`). В логах `CHARGE executed` должен появиться только один раз для этого `orderId`. Повторный `refund` не должен выполнять бизнес-возврат второй раз.

## Как работает идемпотентность

- **PaymentService:** использует `OrderId`, чтобы не провести одну и ту же оплату повторно. Смена суммы с тем же `OrderId` приводит к конфликту.
- **SagaOrchestrator:** использует HTTP-заголовок `Idempotency-Key` и отпечаток запроса. Повтор с тем же ключом и телом возвращает уже полученный результат. Для синхронизации одновременных повторов применяется `ConcurrentDictionary` + `Lazy<Task<SagaResult>>`.

## Ограничения учебной версии

- Состояние хранится **только в памяти** и теряется после перезапуска приложения.
- Защита от повторов действует **внутри одного экземпляра процесса**, а не между несколькими репликами.
- HTTP timeout **не доказывает**, что операция не выполнялась: ответ мог потеряться после изменения состояния.
- При неизвестном результате платежа текущий Orchestrator может не запустить нужный `Refund`; нельзя считать это надёжным recovery-механизмом.
- `SagaIdempotencyStore` кэширует результаты и ошибки; нет политики хранения, TTL и безопасного восстановления зависших операций.
- Отдельные компенсации также могут завершиться ошибкой. Это фиксируется как `CompensationFailed`, но автоматического восстановления пока нет.
- Настоящие платежи не проводятся: `Charge` и `Refund` только меняют In-Memory состояние.

**Важно:** проект предназначен для обучения Saga Pattern, а не для production или обработки реальных платежей.

## Дальнейшие шаги

1. Разобрать частичные сбои, неизвестный статус операций и безопасные повторы.
2. Улучшить управление жизненным циклом Saga и компенсациями.
3. Реализовать Choreography на RabbitMQ + MassTransit.
4. Сравнить синхронную Orchestration и асинхронную Choreography; попробовать гибридный вариант.
