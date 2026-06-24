using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

// Simple in-memory model used by this demo API.
// In a real application, this would usually come from a database or service layer.
public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required string Email { get; set; }

}

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    // Holds all users in memory for this session.
    // This is thread-safe, but it should be replaced with a real data store in production.
    private static readonly ConcurrentDictionary<Guid, User> Users = new();

    [HttpGet("all")]
    public IActionResult GetUsers()
    {
        try
        {
            if (Users.IsEmpty)
            {
                return NotFound(new { Message = "No users found." });
            }
            return Ok(Users.Values);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while retrieving users.", Error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetUserById(Guid id)
    {
        try
        {
            if (Users.TryGetValue(id, out var user))
            {
                return Ok(user);
            }
            return NotFound(new { Message = $"User with id '{id}' was not found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while retrieving the user.", Error = ex.Message });
        }
    }

    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    public IActionResult CreateUser([FromBody] User user)
    {
        // Validate the incoming payload before creating anything.
        if (user == null || string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
        {
            return BadRequest(new { Message = "Name and Email are required." });
        }
        try
        {
            if (user.Id == Guid.Empty)
            {
                // Generate a unique ID only when the client did not provide one.
                Guid newId = Guid.NewGuid();
                while (Users.ContainsKey(newId))
                {
                    newId = Guid.NewGuid();
                }
                user.Id = newId;
            }

            Users[user.Id] = user;
            return Created($"/api/users/{user.Id}", user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while creating the user.", Error = ex.Message });
        }
    }
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public IActionResult UpdateUser(Guid id, [FromBody] User updatedUser)
    {
        // Validate the update payload before changing existing data.
        if (updatedUser == null || string.IsNullOrWhiteSpace(updatedUser.Name) || string.IsNullOrWhiteSpace(updatedUser.Email))
        {
            return BadRequest(new { Message = "Name and Email are required." });
        }
        try
        {
            if (Users.TryGetValue(id, out var existingUser))
            {
                existingUser.Name = updatedUser.Name;
                existingUser.Email = updatedUser.Email;
                return Ok(existingUser);
            }
            return NotFound(new { Message = $"User with id '{id}' was not found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while updating the user.", Error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteUser(Guid id)
    {
        try
        {
            if (Users.TryRemove(id, out _))
            {
                return NoContent();
            }
            return NotFound(new { Message = $"User with id '{id}' was not found." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while deleting the user.", Error = ex.Message });
        }
    }

}