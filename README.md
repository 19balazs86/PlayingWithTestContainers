# Playing with Testcontainers
In this repository you can find an example using minimal API with a few libraries together, but the project mainly focuses on integration testing.

Alba helps to run the API in-memory, while the PostgreSQL server can run in Docker using Testcontainers. After each test, the database is cleaned with the Respawn library, ensuring that each test runs in a clean state.

Additionally, there is a full-text search feature implemented.

#### Resources

- [Testcontainers for .NET](https://dotnet.testcontainers.org/) 📓 - Lightweight implementation to support test environment with Docker containers
  - Using [any container image](https://dotnet.testcontainers.org/api/create_docker_container) in a generic way or [pre-configured modules](https://dotnet.testcontainers.org/modules)
- [Respawn](https://github.com/jbogard/Respawn) 👤*JimmyBogard* - Resets the database back to a clean, empty state
- [Mapster](https://github.com/MapsterMapper/Mapster) 👤 - Object to object mapper
- [Alba](https://jasperfx.github.io/alba) 📓 - Utilizes the built-in `TestServer`
- [The cleanest way to use Docker for testing](https://youtu.be/8IRNC7qZBmk) 📽️*13min-NickChapsas*
- [Reset your database during testing with Respawn](https://youtu.be/E4TeWBFzcCw) 📽️*13min-NickChapsas*
- [Testing RabbitMQ with Testcontainers](https://youtu.be/DMs3ZuakHGA) 📽️*12 min - Gui Ferreira*
---
- [Integration test using SqlServer image with TestContainers](https://hamidmosalla.com/2022/09/10/integration-test-in-asp-net-core-6-using-sqlserver-image-and-testcontainers) 📓*HamidMosalla*
- [Minimal API validation with Endpoint Filters](https://benfoster.io/blog/minimal-api-validation-endpoint-filters) 📓*BenFoster* | [FluentValidation](https://docs.fluentvalidation.net)
---

- [FluentDocker](https://github.com/mariotoffia/FluentDocker) 👤*Mario Toffia* - Other library that enables docker and docker-compose interactions
