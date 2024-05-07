using System.ComponentModel.DataAnnotations;

namespace Kolokwium1.DTOs;

public record CreateBookDTO
(
    [Required] string Title,
    [Required][MinLength(1)] IEnumerable<AuthorDTO> Authors
    );
    
    