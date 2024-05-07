using System.ComponentModel.DataAnnotations;

namespace Kolokwium1.DTOs;

public record BookRequestDTO
(
    int ID,
    string Title,
    List<AuthorDTO> Authors
    );
    
public record AuthorDTO(
        [Required]string FirstName,
        [Required]string LastName
        );