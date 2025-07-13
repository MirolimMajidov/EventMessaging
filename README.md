## There are two libraries

### 1. EventBus.RabbitMQ
EventBus.RabbitMQ is a messaging library designed to simplify the implementation of communication using RabbitMQ. It enables seamless publishing and receiving of events between microservices or other types of applications. The library is easy to set up and is compatible with .NET8 or recent frameworks. Additionally, it supports working with multiple virtual hosts in RabbitMQ.

With this library, you can easily implement the [Inbox and outbox patterns](https://en.wikipedia.org/wiki/Inbox_and_outbox_pattern) in your application. It allows you to persist all incoming and outgoing event messages in the database. Currently, it supports storing event data only in a PostgreSQL database.

### NuGet package
[![Version](https://img.shields.io/nuget/v/Mirolim.EventBus.RabbitMQ?label=Version:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)

#### [See the EventBus.RabbitMQ documentation for more information](https://github.com/MirolimMajidov/EventMessaging/tree/main/src/EventBus.RabbitMQ#eventbusrabbitmq)

### 2. EventStorage
EventStorage is a library designed to simplify the implementation of the [Inbox and outbox patterns](https://en.wikipedia.org/wiki/Inbox_and_outbox_pattern) for handling multiple types of events in your application. It allows you to persist all incoming and outgoing event messages in the database. Currently, it supports storing event data only in a PostgreSQL database.

### NuGet package
[![Version](https://img.shields.io/nuget/v/Mirolim.EventStorage?label=Version:Mirolim.EventStorage)](https://www.nuget.org/packages/Mirolim.EventStorage)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventStorage?label=Downloads:Mirolim.EventStorage)](https://www.nuget.org/packages/Mirolim.EventStorage)

#### [See the EventStorage documentation for more information](https://github.com/MirolimMajidov/EventMessaging/tree/main/src/EventStorage#eventstorage)