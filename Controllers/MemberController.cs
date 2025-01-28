using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace dotent.Controllers;

public class MemberController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public MemberController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static List<Member> Members = new List<Member>();

    [HttpPost("/api/member")]
    public async Task<IActionResult> Create([FromBody] MemberDto memberDto)
    {
        try
        {
            const string insertQuery = $"""
                                        insert into members (first_name, email, phone, address)
                                        values(@firstName, @email, @phone, @address) 
                                        """;
            var connectionString = _configuration.GetConnectionString("Default");

            await using var connection = new NpgsqlConnection(connectionString);

            await connection.ExecuteAsync(insertQuery, new
            {
                firstName = memberDto.FirstName,
                email = memberDto.Email,
                phone = memberDto.PhoneNumber,
                address = memberDto.Address
            });
            return Ok("Member created successfully");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("/api/member/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            const string selectQuery = $"""
                                        select * from members where id = @id 
                                        """;
            var connectionString = _configuration.GetConnectionString("Default");

            await using var connection = new NpgsqlConnection(connectionString);

            var member = await connection.QueryFirstOrDefaultAsync<Member>(selectQuery,
                new
                {
                    id = id
                });

            if (member == null)
            {
                return NotFound("Member not found");
            }

            return Ok(member);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }


    [HttpGet("/api/member")]
    public IActionResult GetAll([FromQuery] MemberFilterDto filter)
    {
        var members = Members.Where(x =>
            (string.IsNullOrEmpty(filter.FirstName) || x.First_Name.Contains(filter.FirstName))
            && (string.IsNullOrEmpty(filter.Address) || x.Address.Contains(filter.Address))
        ).ToList();
        return Ok(members);
    }


    [HttpPut("/api/member/{id}")]
    public IActionResult Update(int id, [FromBody] MemberDto memberDto)
    {
        var exisingMember = Members.FirstOrDefault(x => x.Id == id);

        if (exisingMember == null)
        {
            return NotFound();
        }

        exisingMember.First_Name = memberDto.FirstName;
        exisingMember.Email = memberDto.Email;
        exisingMember.Phone = memberDto.PhoneNumber;
        exisingMember.Address = memberDto.Address;

        return Ok("Member updated successfully");
    }

    [HttpDelete("/api/member/{id}")]
    public IActionResult Delete(int id)
    {
        var member = Members.FirstOrDefault(x => x.Id == id);
        if (member == null)
        {
            return NotFound();
        }

        Members.Remove(member);
        return Ok("Member deleted successfully");
    }
}

public class MemberFilterDto
{
    public string? FirstName { get; set; }
    public string? Address { get; set; }
}

public class MemberDto // Data Transfer Object
{
    public string FirstName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}

public class Member
{
    public int Id { get; set; }
    public string First_Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}