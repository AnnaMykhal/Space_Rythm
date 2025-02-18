﻿using System.ComponentModel.DataAnnotations;

namespace SpaceRythm.Models.User;

public class CreateUserRequest
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}

