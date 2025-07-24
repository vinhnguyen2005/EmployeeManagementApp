﻿using System;
using System.Collections.Generic;

namespace ConsoleApp.Models;

public partial class Location
{
    public int LocationId { get; set; }

    public string? StreetAddress { get; set; }

    public string? PostalCode { get; set; }

    public string City { get; set; } = null!;

    public string? StateProvince { get; set; }

    public string CountryId { get; set; } = null!;

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
