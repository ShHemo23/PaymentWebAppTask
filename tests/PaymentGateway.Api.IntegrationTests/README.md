# Integration Tests Setup

This document explains how to run the integration tests for the Payment Gateway API.

## Database Options

The test suite supports two database modes:

1. **Docker Container (Default)**
   - Uses TestContainers to spin up a SQL Server instance in Docker
   - Requires Docker to be installed and running
   - Automatically handles container lifecycle
   - Preferred for CI/CD environments

2. **LocalDB Fallback**
   - Uses SQL Server LocalDB
   - Automatically used if Docker container setup fails
   - Can be explicitly enabled by setting environment variable
   - Useful for development environments without Docker

## Configuration

### Environment Variables

1. **USE_LOCAL_SQL_SERVER**
   - Set to "true" to force using LocalDB instead of Docker
   - Example: `USE_LOCAL_SQL_SERVER=true dotnet test`

2. **Docker Authentication**
   - The system automatically tries different authentication methods for MCR
   - No manual configuration needed in most cases
   - Supports both credential-based and anonymous access

### Prerequisites

1. For Docker mode:
   - Docker installed and running
   - Internet access to pull SQL Server image

2. For LocalDB mode:
   - SQL Server LocalDB installed
   - Windows environment

## Logging

- Detailed logs are available during test execution
- Logs include database initialization steps
- Container lifecycle events are logged
- Authentication attempts are tracked

## Troubleshooting

1. **Docker Authentication Issues**
   - The system will automatically try multiple authentication methods
   - If all fail, it will fall back to LocalDB
   - Check Docker daemon is running
   - Verify network connectivity to MCR

2. **Database Connection Issues**
   - Verify LocalDB is installed if using local mode
   - Check port availability for Docker mode
   - Review logs for specific error messages

## Best Practices

1. **CI/CD Environments**
   - Use Docker mode for consistent testing environment
   - Ensure proper Docker configuration in CI/CD pipeline

2. **Local Development**
   - Use LocalDB mode if Docker is not needed
   - Faster startup times for frequent test runs

## Example Usage

```bash
# Run tests with Docker (default)
dotnet test

# Run tests with LocalDB
SET USE_LOCAL_SQL_SERVER=true
dotnet test
``` 