# Task Management System

A .NET 8 Web API application that implements a Task Management System with RabbitMQ integration using Clean Architecture principles.

## Features

- Create, update, and view tasks
- Task status management with logical transitions
- RabbitMQ integration for task completion events
- SQL Server database for task storage
- Docker support for easy deployment
- API versioning support
- Comprehensive health checks
- Structured logging with Serilog and Seq
- Swagger/OpenAPI documentation
- Health check UI dashboard
- Message retry and resilience patterns
- Container orchestration support

## Architecture

The application follows Clean Architecture principles with the following layers:

- **Domain**: Contains core business logic, entities, and interfaces
- **Application**: Contains application-specific business rules and DTOs
- **Infrastructure**: Contains implementations of interfaces and external services
- **API**: Contains the Web API controllers and configuration

## Prerequisites

- Docker Desktop
- .NET 8 SDK (for local development)
- Seq (for log aggregation, optional)
- Kubernetes (for production deployment, optional)

## Setup and Running

1. Clone the repository
2. Navigate to the project directory
3. Run the following command in docker terminal or powershell to start all services:

```bash
docker-compose up --build -d
```

The application will be available at:
- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- Health Check UI: http://localhost:8080/health-ui
- RabbitMQ Management: http://localhost:15672 (guest/guest)
- SQL Server: localhost,1433 (sa/Your_password123)
- Seq (if running): http://localhost:5341

## API Endpoints

### Version 1.0
- `POST /api/v1/tasks` - Create a new task
- `PUT /api/v1/tasks/{id}/status` - Update task status
- `GET /api/v1/tasks` - Get all tasks
- `GET /api/v1/tasks/{id}` - Get task by ID

### Health Checks
- `GET /health` - Health check endpoint
- `GET /health-ui` - Health check dashboard
- `GET /health-api` - Health check API

## Design Decisions and Trade-offs

1. **Clean Architecture**: Chosen for separation of concerns and maintainability, but adds some complexity in project structure.

2. **MassTransit**: Used for message bus abstraction, providing a clean interface for RabbitMQ integration.

3. **Entity Framework Core**: Used for data access, providing easy database operations but adding some overhead.

4. **Docker Compose**: Used for easy deployment and development, but requires Docker knowledge.

5. **Status Transitions**: Implemented strict status transitions to ensure data consistency.

6. **API Versioning**: Implemented to support future API changes while maintaining backward compatibility.

7. **Health Checks**: Comprehensive health monitoring of system resources and dependencies.

8. **Structured Logging**: Using Serilog with Seq for better log management and analysis.

## Technical Features

1. **API Versioning**
   - URL-based versioning (e.g., /api/v1/tasks)
   - Query string versioning (e.g., ?api-version=1.0)
   - Header-based versioning (X-API-Version: 1.0)

2. **Health Monitoring**
   - System resource monitoring (CPU, Memory)
   - Database health checks
   - RabbitMQ health checks
   - Custom health checks
   - Health check UI dashboard

3. **Logging**
   - Structured logging with Serilog
   - Console and Seq sinks
   - Request/response logging
   - Error logging with stack traces

4. **Error Handling**
   - Global exception handling middleware
   - Custom exception types
   - Consistent error responses
   - Detailed error logging

5. **Containerization**
   - Docker support for all components
   - Multi-stage builds for optimized images
   - Environment-specific configurations
   - Health check probes for container orchestration

## Limitations

1. No authentication/authorization implemented
2. No caching layer
3. Limited horizontal scaling in current setup

## Future Improvements

1. **Security**
   - Add authentication and authorization
   - Implement API key management
   - Add rate limiting
   - Implement security headers

2. **Performance**
   - Implement pagination for task listing
   - Add caching layer
   - Add distributed caching
   - Implement query optimization

3. **Monitoring and Observability**
   - Add comprehensive unit and integration tests
   - Implement distributed tracing
   - Add performance monitoring
   - Implement metrics collection

4. **Resilience**
   - Implement circuit breakers for external services
   - Add service mesh integration
   - Implement automatic failover
   - Add chaos engineering tests

5. **Scaling**
   - Implement Kubernetes deployment
   - Add auto-scaling policies
   - Implement database sharding
   - Add read replicas

## Testing

The solution includes both unit tests and integration tests:

### Unit Tests
- Located in `TaskManagement.UnitTests` project
- Uses xUnit as the testing framework
- Uses Moq for mocking dependencies
- Uses FluentAssertions for more readable assertions
- Follows AAA (Arrange-Act-Assert) pattern
- Tests domain entities, application services, and business logic

### Integration Tests
- Located in `TaskManagement.API.Tests` project
- Tests API endpoints and database operations
- Uses TestContainers for isolated testing environment
- Tests real database interactions and message bus operations

### Running Tests
To run all tests:
```bash
dotnet test
```

To run tests with coverage report:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage
The project uses Coverlet for code coverage reporting. Coverage reports are generated in the `TestResults` directory after running tests with coverage collection enabled.