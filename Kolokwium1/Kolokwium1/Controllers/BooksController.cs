using Kolokwium1.DTOs;
using Kolokwium1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers;


[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;

    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }

    [HttpGet("{bookId:int}/authors")]
    public async Task<IActionResult> GetBookWithAuthors(int bookId)
    {
        var result = await _booksRepository.GetBookWithAuthors(bookId);
        if (result is not null) return Ok(result);
        return NotFound($"Book with id:{bookId} not found");
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookDTO request)
    {
        var result = await _booksRepository.CreateBookWithAuthors(request);
        if (result is not null) return Created("",result);
        return Conflict($"Error creating book {request.Title}");
    }
}