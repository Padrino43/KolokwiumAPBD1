using System.Data.SqlClient;
using Kolokwium1.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Repositories;

public interface IBooksRepository
{
    Task<BookRequestDTO?> GetBookWithAuthors(int bookId);
    Task<List<AuthorDTO>?> GetAuthors(int bookId);
    Task<BookRequestDTO?> CreateBookWithAuthors(CreateBookDTO bookWithAuthors);
}

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;

    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<BookRequestDTO?> GetBookWithAuthors(int bookId)
    {
        await using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var sqlCommand = new SqlCommand("SELECT PK, title FROM books WHERE PK = @Id"
            , sqlConnection);
        sqlCommand.Parameters.AddWithValue("@Id", bookId);
        await sqlConnection.OpenAsync();
        var reader = await sqlCommand.ExecuteReaderAsync();
        
        if (!reader.HasRows) return null;


        var authors = await GetAuthors(bookId);

        await reader.ReadAsync();
        var result = new BookRequestDTO(
            reader.GetInt32(0), 
            reader.GetString(1), 
           authors
        );
        
        
        return result;
    }
    
    public async Task<List<AuthorDTO>?> GetAuthors(int bookId)
    {
        await using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var sqlCommand = new SqlCommand(@"SELECT a.first_name, a.last_name FROM authors a
                JOIN books_authors ba ON a.PK = ba.FK_author WHERE ba.FK_book = @Id"
            , sqlConnection);
        sqlCommand.Parameters.AddWithValue("@Id", bookId);
        await sqlConnection.OpenAsync();
        var reader = await sqlCommand.ExecuteReaderAsync();
        
        if (!reader.HasRows) return null;

        var authors = new List<AuthorDTO>();
        
        
        while (await reader.ReadAsync())
        {
            authors.Add(
                new AuthorDTO(
                    reader.GetString(0),
                    reader.GetString(1))
                );
        }
        
        
        return authors;
    }
    
    public async Task<BookRequestDTO?> CreateBookWithAuthors(CreateBookDTO bookWithAuthors)
    {
        await using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("INSERT INTO books(title) VALUES (@title); SELECT SCOPE_IDENTITY() as int"
            , sqlConnection);
    
        await sqlConnection.OpenAsync();
        var transaction = sqlConnection.BeginTransaction();
        try
        {
            sqlCommand.Parameters.AddWithValue("@title", bookWithAuthors.Title);
            sqlCommand.Transaction = transaction;
            
            var bookId = await sqlCommand.ExecuteScalarAsync();
            var authorsId = new List<int>();
            
            sqlCommand.CommandText =
                @"INSERT INTO authors (first_name, last_name)
               VALUES (@firstName, @lastName);SELECT SCOPE_IDENTITY();";
            foreach (var author in bookWithAuthors.Authors)
            {
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.AddWithValue("@firstName", author.FirstName);
                sqlCommand.Parameters.AddWithValue("@lastName", author.LastName);
                authorsId.Add(Convert.ToInt32(await sqlCommand.ExecuteScalarAsync()));
            }
            
            sqlCommand.CommandText =
                @"INSERT INTO books_authors (FK_book, FK_author)
               VALUES (@bookId, @authorId);";
            foreach (var authorId in authorsId)
            {
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.AddWithValue("@bookId", Convert.ToInt32(bookId));
                sqlCommand.Parameters.AddWithValue("@authorId", authorId);
                await sqlCommand.ExecuteScalarAsync();
            }
    
            await transaction.CommitAsync();
            
            return await GetBookWithAuthors(Convert.ToInt32(bookId));
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return null;
        }
    }
}