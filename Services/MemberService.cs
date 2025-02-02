using Dapper;
using dotent.Controllers;
using Npgsql;

namespace dotent.Services;

public class MemberService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public MemberService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("Default")!;
    }

    public async Task CreateAsync(MemberDto memberDto)
    {
        const string insertQuery = $"""
                                    insert into members (first_name, email, phone, address)
                                    values(@firstName, @email, @phone, @address) 
                                    """;

        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.ExecuteAsync(insertQuery, new
        {
            firstName = memberDto.FirstName,
            email = memberDto.Email,
            phone = memberDto.PhoneNumber,
            address = memberDto.Address
        });
    }

    public async Task DeleteAsync(int id)
    {
        const string deleteQuery = $"""
                                    delete from members where id = @id
                                    """;

        var connectionString = _configuration.GetConnectionString("Default");

        await using var connection = new NpgsqlConnection(connectionString);

        await connection.ExecuteAsync(deleteQuery, new
        {
            id = id
        });
    }
}