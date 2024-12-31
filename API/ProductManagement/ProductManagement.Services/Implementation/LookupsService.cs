using System.Data;
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using ProductManagement.EFCore.Abstractions;
using ProductManagement.EFCore.Models;
using ProductManagement.EFCore.Shared;
using ProductManagement.Services.Interfaces;

namespace ProductManagement.Services.Implementation;

public class LookupsService : ILookupsService
{
    private readonly ProductManagementDBContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public LookupsService(ProductManagementDBContext dbContext, IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    /// <summary>
    /// Retrieves all records from the specified lookup table asynchronously.
    /// </summary>
    /// <param name="tableName">The name of the lookup table to query.</param>
    /// <returns>A list of <see cref="Lookups"/> containing the records from the specified lookup table.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid table name is provided.</exception>
    public List<Lookups> GetAllAsync(string tableName)
    {
        if (!IsValidTableName(tableName))
            throw new ArgumentException("Invalid table name");

        using var connection = new SqlConnection() { ConnectionString = _configuration.GetConnectionString("DefaultConnection")! };
        connection.Open();

        using var command = connection.CreateCommand();
        if (tableName == "PaymentsTypes")
            command.CommandText = $"SELECT * FROM [lookups].[{tableName}] WHERE OrderIndex IS NOT NULL";
        else
            command.CommandText = $"SELECT * FROM [lookups].[{tableName}]";

        using var reader = command.ExecuteReader();
        var rows = new List<Lookups>();
        while (reader.Read())
        {
            var row = new Lookups
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            };
            if (!reader.IsDBNull(reader.GetOrdinal("OrderIndex")))
                row.OrderIndex = reader.GetInt32(reader.GetOrdinal("OrderIndex"));
            else
                row.OrderIndex = null;
            rows.Add(row);
        }
        rows = rows.OrderBy(x => x.OrderIndex.HasValue ? 0 : 1)
                   .ThenBy(x => x.OrderIndex)
                   .ThenBy(x => x.Id)
                   .ToList();
        return rows;
    }

    /// <summary>
    /// Retrieves a specific record from the specified lookup table asynchronously based on the given identifier.
    /// </summary>
    /// <param name="tableName">The name of the lookup table to query.</param>
    /// <param name="id">The identifier of the record to retrieve.</param>
    /// <returns>
    /// A <see cref="Lookups"/> instance representing the record with the specified identifier,
    /// or <c>null</c> if no record is found.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when an invalid table name is provided.</exception>
    public async Task<Lookups?> GetByIdAsync(string tableName, int id)
    {
        if (!IsValidTableName(tableName))
            throw new ArgumentException("Invalid table name");

        var query = $"SELECT * FROM [lookups].[{tableName}] WHERE Id = {id}";
        using var command = _dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;

        await _dbContext.Database.OpenConnectionAsync();
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var row = new Lookups();
            row.Id = reader.GetInt32(reader.GetOrdinal("Id"));
            row.Name = reader.GetString(reader.GetOrdinal("Name"));
            if (!reader.IsDBNull(reader.GetOrdinal("OrderIndex")))
                row.OrderIndex = reader.GetInt32(reader.GetOrdinal("OrderIndex"));
            else
                row.OrderIndex = null;
            return row;
        }
        else
            return null;
    }

    /// <summary>
    /// Creates a new record in the specified lookup table asynchronously based on the provided information.
    /// </summary>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="tableName">The name of the lookup table to insert the record into.</param>
    /// <param name="lookups">An instance of <see cref="LookupsEdit"/> containing the data for the new record.</param>
    /// <returns>
    /// A string indicating the result of the operation:
    /// - "Added" if the record is successfully added.
    /// - <c>null</c> if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when an invalid table name is provided.</exception>
    public async Task<string?> CreateAsync(string userId, string tableName, LookupsEdit lookups)
    {
        if (!IsValidTableName(tableName))
            throw new ArgumentException("Invalid table name");
        var columns = "[Name], [OrderIndex]";
        var values = "@Name, @OrderIndex";
        var parameters = new List<SqlParameter>
    {
        new SqlParameter("@Name", lookups.Name),
        new SqlParameter("@OrderIndex", lookups.OrderIndex ?? (object)DBNull.Value)
    };
        var query = $"INSERT INTO [lookups].[{tableName}] ({columns}) VALUES ({values})";
        var result = await _dbContext.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        if (result != 1)
            return null;

        var auditRecord = new Audit
        {
            UserId = userId,
            TableName = "Lookups",
            Type = "Create",
            DateTime = DateTime.UtcNow,
            OldValues = null,
            NewValues = $"{{\"Name\":\"{lookups.Name}\",\"OrderIndex\":\"{(lookups.OrderIndex == null ? null : lookups.OrderIndex)}\"}}",
            AffectedColumns = null,
            PrimaryKey = null
        };
        //_ProductManagementAuditsDBContext.Audits.Add(auditRecord);
        //await _ProductManagementAuditsDBContext.SaveChangesAsync();

        return "Added";
    }

    /// <summary>
    /// Updates an existing record in the specified lookup table asynchronously based on the provided information.
    /// </summary>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="tableName">The name of the lookup table to update the record in.</param>
    /// <param name="id">The identifier of the record to be updated.</param>
    /// <param name="lookups">An instance of <see cref="LookupsEdit"/> containing the updated data for the record.</param>
    /// <returns>
    /// A string indicating the result of the operation:
    /// - "Updated" if the record is successfully updated.
    /// - <c>null</c> if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when an invalid table name is provided.</exception>
    public async Task<string?> UpdateAsync(string userId, string tableName, int id, LookupsEdit lookups)
    {
        if (!IsValidTableName(tableName))
            throw new ArgumentException("Invalid table name");

        var query = $"UPDATE [lookups].[{tableName}] SET [Name] = @Name, [OrderIndex] = @OrderIndex WHERE [Id] = @Id";
        var parameters = new List<SqlParameter>
    {
        new SqlParameter("@Name", lookups.Name),
        new SqlParameter("@Id", id),
        new SqlParameter("@OrderIndex", lookups.OrderIndex ?? (object)DBNull.Value)
    };
        var result = await _dbContext.Database.ExecuteSqlRawAsync(query, parameters.ToArray());
        if (result != 1)
            return null;

        var auditRecord = new Audit
        {
            UserId = userId,
            TableName = "Lookups",
            Type = "Update",
            DateTime = DateTime.UtcNow,
            OldValues = "",
            NewValues = $"{{\"Name\":\"{lookups.Name}\",\"OrderIndex\":{(lookups.OrderIndex == null ? "null" : $"\"{lookups.OrderIndex}\"")}}}",
            AffectedColumns = $"{{\"Name\",\"OrderIndex\"",
            PrimaryKey = $"{{\"Id\":\"{id}\"}}"
        };

        //_ProductManagementAuditsDBContext.Audits.Add(auditRecord);
        //await _ProductManagementAuditsDBContext.SaveChangesAsync();

        return "Updated";
    }

    /// <summary>
    /// Deletes a record from the specified lookup table asynchronously based on the provided identifier.
    /// </summary>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="tableName">The name of the lookup table from which the record will be deleted.</param>
    /// <param name="id">The identifier of the record to be deleted.</param>
    /// <returns>
    /// A string indicating the result of the operation:
    /// - "Deleted" if the record is successfully deleted.
    /// - <c>null</c> if the operation fails.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when an invalid table name is provided.</exception>
    public async Task<string?> DeleteAsync(string userId, string tableName, int id)
    {
        if (!IsValidTableName(tableName))
            throw new ArgumentException("Invalid table name");
        var query = $"DELETE FROM [lookups].[{tableName}] WHERE Id = {id}";
        var result = await _dbContext.Database.ExecuteSqlRawAsync(query);
        if (result != 1)
            return null;

        var auditRecord = new Audit
        {
            UserId = userId,
            TableName = "Lookups",
            Type = "Delete",
            DateTime = DateTime.UtcNow,
            OldValues = null,
            NewValues = null,
            AffectedColumns = null,
            PrimaryKey = $"{{\"Id\":\"{id}\"}}"
        };

        //_ProductManagementAuditsDBContext.Audits.Add(auditRecord);
        //await _ProductManagementAuditsDBContext.SaveChangesAsync();

        return "Deleted";
    }

    /// <summary>
    /// Retrieves a collection of table names from the specified database schema, excluding specific tables.
    /// </summary>
    /// <param name="schemaName">The name of the database schema from which to retrieve table names.</param>
    /// <returns>
    /// An IEnumerable&lt;string&gt; containing the names of tables in the specified schema, excluding certain tables.
    /// </returns>
    /// <remarks>
    /// Excludes tables with names "Ages," "Bmi," and "OrderStates" from the result.
    /// </remarks>
    public IEnumerable<string> GetTableNames(string schemaName)
    {
        var TableNames = _unitOfWork.Repository<object>().GetTableNamesInSchema(schemaName) ?? new List<string>();
        return TableNames.Where(n => n != "Ages" && n != "Bmi" && n != "OrderStates" && n != "PaymentsTypes" && n != "InvoiceTypes" && n != "RefundStates");
    }

    /// <summary>
    /// Checks whether the provided table name is valid, allowing only alphanumeric characters.
    /// </summary>
    /// <param name="tableName">The table name to validate.</param>
    /// <returns>
    /// <c>true</c> if the table name is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The method uses a regular expression to ensure that the table name consists only of alphabetical characters (both upper and lower case).
    /// </remarks>
    private bool IsValidTableName(string tableName)
    {
        return Regex.IsMatch(tableName, "^[a-zA-Z0-9]+$");
    }
}
