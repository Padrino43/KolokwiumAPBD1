using Kolokwium1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers;


[ApiController]
[Route("api/[controller]")]
public class Controller : ControllerBase
{
    private readonly IxRepository _xRepository;

    public Controller(IxRepository xRepository)
    {
        _xRepository = xRepository;
    }

    // [HttpDelete("{id:int}")]
    // public async Task<IActionResult> Remove(int id)
    // {
    //     var result = await db.RemoveStudentByIdAsync(id);
    //     if (result) return NoContent();
    //     return NotFound($"Student with id:{id} does not exist");
    // }
}