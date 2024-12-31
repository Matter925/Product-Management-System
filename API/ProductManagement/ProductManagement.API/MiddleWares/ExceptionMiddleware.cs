using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using ProductManagement.API.ResponseModule;

namespace ProductManagement.API.MiddleWares;

public class ExceptionMiddleware(RequestDelegate next, IHostEnvironment hostEnvironment, ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        if (exception is DbUpdateException dbUpdateException && dbUpdateException.InnerException is SqlException sqlException)
        {
            var response = HandleSqlException(sqlException);
            context.Response.StatusCode = response.StatusCode;
            await WriteResponseAsync(context, response);
        }
        else
        {
            var response = HandleGeneralException(exception);
            context.Response.StatusCode = response.StatusCode;
            await WriteResponseAsync(context, response);
        }
    }

    private async Task WriteResponseAsync(HttpContext context, ApiException response)
    {
        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }

    private ApiException HandleSqlException(SqlException sqlException)
    {
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string message = sqlException.Number switch
        {
            2601 or 2627 => GetUniqueConstraintErrorMessage(sqlException.Message),
            547 => GetForeignKeyConstraintErrorMessage(sqlException.Message),
            _ => "An unknown database error occurred. Please contact support.",
        };
        _logger.LogWarning("SQL Exception: {SqlErrorNumber} - {SqlErrorMessage}", sqlException.Number, sqlException.Message);
        return new ApiException(statusCode, message);
    }

    private string GetUniqueConstraintErrorMessage(string sqlMessage)
    {
        var pattern = @"with unique index '(?<indexName>.*?)'\.";
        var match = Regex.Match(sqlMessage, pattern);

        if (match.Success)
        {
            var fullUniqueIndex = match.Groups["indexName"].Value;
            var columnName = ExtractColumnNameFromIndex(fullUniqueIndex);
            return $"The value for '{columnName}' is already in use. Please provide a unique value.";
        }

        return "A unique constraint violation occurred.";
    }

    private string GetForeignKeyConstraintErrorMessage(string sqlMessage)
    {
        if (sqlMessage.Contains("DELETE"))
            return "This item cannot be deleted because it is referenced by other records.";

        var tableName = ExtractTableName(sqlMessage);
        return $"The field related to '{tableName}' is required and cannot be empty.";
    }

    private ApiException HandleGeneralException(Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var guid = Guid.NewGuid();
        if (_hostEnvironment.IsDevelopment())
        {
            var detailedMessage = BuildDetailedExceptionMessage(exception);
            _logger.LogCritical("Critical Exception [{Guid}] => {Message}", guid, detailedMessage);
            return new ApiException((int)code, $"[{guid}] => {detailedMessage}", exception.StackTrace);
        }
        else
        {
            _logger.LogError("Exception [{Guid}] => {Message}", guid, exception.Message);
            return new ApiException((int)code, $"Error Code: {guid}. Please contact support.");
        }
    }

    private string BuildDetailedExceptionMessage(Exception exception)
    {
        var sb = new StringBuilder(exception.Message);
        while (exception.InnerException != null)
        {
            exception = exception.InnerException;
            sb.Append(" => ").Append(exception.Message);
        }
        return sb.ToString();
    }

    private static string ExtractTableName(string errorMessage)
    {
        var pattern = @"table ""dbo\.(?<tableName>.*?)""";
        var match = Regex.Match(errorMessage, pattern);

        if (match.Success)
        {
            var tableName = match.Groups["tableName"].Value;
            return tableName.EndsWith('s') ? tableName.Substring(0, tableName.Length - 1) : tableName;
        }

        return "the relevant fields";
    }

    private static string ExtractColumnNameFromIndex(string fullIndex)
    {
        var parts = fullIndex.Split('_');
        return parts.LastOrDefault() ?? "Unknown";
    }
}