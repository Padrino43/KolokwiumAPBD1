using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Repositories;

public interface IxRepository
{
    
}

public class xRepository : IxRepository
{
    private readonly IConfiguration _configuration;

    public xRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ActionResult> Getx(int id)
    {
        await using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("SELECT "
            , sqlConnection);
        sqlCommand.Parameters.AddWithValue("@Id", id);
        await sqlConnection.OpenAsync();
        var reader = await sqlCommand.ExecuteReaderAsync();
        await reader.ReadAsync();
        return null;
    }
    
    public async Task<int?> CreateProduct(int idWarehouse, int idProduct, int idOrder, double price, int amount, DateTime createdAt)
    {
        await using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("UPDATE \"Order\" SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder"
            , sqlConnection);

        await sqlConnection.OpenAsync();
        var transaction = sqlConnection.BeginTransaction();
        try
        {
            sqlCommand.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@IdOrder", idOrder);
            sqlCommand.Transaction = transaction;
            await sqlCommand.ExecuteNonQueryAsync();

            sqlCommand.CommandText =
                @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, CreatedAt, Amount, Price)
               VALUES (@IdWarehouse, @IdProduct, @IdOrder, @CreatedAt, @Amount, @Price);SELECT SCOPE_IDENTITY();";
            sqlCommand.Parameters.Clear();
            sqlCommand.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            sqlCommand.Parameters.AddWithValue("@IdProduct", idProduct);
            sqlCommand.Parameters.AddWithValue("@IdOrder", idOrder);
            sqlCommand.Parameters.AddWithValue("@CreatedAt", createdAt);
            sqlCommand.Parameters.AddWithValue("@Amount", amount);
            sqlCommand.Parameters.AddWithValue("@Price", price * amount);
            var idProductWarehouse =  Convert.ToInt32(await sqlCommand.ExecuteScalarAsync());

            await transaction.CommitAsync();
            return idProductWarehouse;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return null;
        }
    }
}