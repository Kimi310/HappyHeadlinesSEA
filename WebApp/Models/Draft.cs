using System;
using System.Collections.Generic;

namespace DraftService.Models;

public partial class Draft
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Continent { get; set; } = null!;

    public bool IsGlobal { get; set; }
}
