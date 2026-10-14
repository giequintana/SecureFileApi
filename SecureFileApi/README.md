
# SecureFileApi

A RESTful ASP.NET Core Web API that securely accepts CSV uploads,
calculates average salary, and tracks file processing history.

## Requirements

- Visual Studio 2022
- .NET 8 SDK (or the framework selected by the project)
- Postman or another HTTP client

## Features

- API key authentication using X-API-Key
- CSV file upload and validation
- Average salary calculation
- File processing history and total count
- Structured logging and error handling
- Maximum upload size of 5 MB

## Build and Run

1. Open SecureFileApi.sln in Visual Studio.
2. Configure the FILE_API_KEY environment variable.
3. Build the solution using Build > Build Solution.
4. Run using F5 or Ctrl+F5.

Alternatively, run from a terminal:

```powershell
$env:FILE_API_KEY = "gq-api-key"
dotnet restore
dotnet build
dotnet run --urls http://localhost:5000
```

Use HTTPS for deployments and real environments.

## Authentication

All /api endpoints require this header:

X-API-Key: gq-api-key

Requests without a valid key return HTTP 401.

## CSV Format

The CSV must have these columns in this order:

```csv
Id,Name,Department,Salary
1,Alice,IT,50000
2,Bob,HR,45000
3,Charlie,Finance,55000
```

Average salary is calculated from valid rows only.

## API Endpoints

### 1. Upload a file

POST /api/files/upload

Content-Type: multipart/form-data

Form field:
- file: the CSV file to process

Example using curl:

```bash
curl -X POST "http://localhost:5000/api/files/upload" \
  -H "X-API-Key: gq-api-key" \
  -F "file=@employees.csv"
```

Successful HTTP response: 200 OK.

The response contains the processing ID, filename, row counts,
average salary, errors, status, and processing timestamp.

### 2. View processing history

GET /api/files/history

Returns the number of tracked processing attempts and the history records.

```bash
curl "http://localhost:5000/api/files/history" \
  -H "X-API-Key: gq-api-key"
```

### 3. Get one processing record

GET /api/files/history/{id}

Replace {id} with the ID returned by the upload endpoint.

Returns HTTP 404 if the record does not exist.

## Error Handling

- 400 Bad Request: Missing file, empty file, or unsupported extension
- 401 Unauthorized: Missing or invalid API key
- 404 Not Found: Unknown processing record
- 413 Payload Too Large: Upload exceeds the configured size limit
- 500 Internal Server Error: Unexpected upload failure

CSV validation failures are returned as a processing result with
status Failed or CompletedWithErrors.

## Logging and Tracking

The application logs upload activity, processing outcomes, and
unexpected errors.

File history is held in memory and is cleared when the application
restarts. A production version should persist the history in SQL Server.

## Security Notes

- Store real API keys outside source control.
- Use HTTPS.
- Configure appropriate server-level request size limits.
- Add rate limiting and API key rotation for production use.
- Do not log API keys or sensitive file contents.
