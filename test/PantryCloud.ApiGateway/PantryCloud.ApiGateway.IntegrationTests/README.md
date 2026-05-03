# Gateway Testing Guide

### Integration Tests

Integration tests are currently implemented as manual tests.

### Rate Limiting
1. Start the Gateway: `docker compose up api-gateway`
2. Make multiple requests to `/api/health`:
   ```bash
   for i in {1..105}; do curl http://localhost:5050/api/health; done
   ```
3. After the configured limit (default: 100 requests per 60 seconds), you should receive `429 Too Many Requests`

### Correlation IDs
1. Make a request without correlation ID:
   ```bash
   curl -v http://localhost:5050/api/health
   ```
   Check response headers for `X-Correlation-Id`

2. Make a request with correlation ID:
   ```bash
   curl -v -H "X-Correlation-Id: test-123" http://localhost:5050/api/health
   ```
   Verify the same correlation ID is returned in response headers

### Request Logging
Check Gateway logs to see structured logging with:
- Request method and path
- Remote IP address
- User (authenticated user ID or "anonymous")
- Correlation ID
- Response status code
- Request duration
