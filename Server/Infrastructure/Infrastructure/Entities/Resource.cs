namespace Infrastructure.Entities;

using Infrastructure.Base;
using System.ComponentModel.DataAnnotations;
using static Domain.Entities.Directories.Resource;

public class Resource : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public Conditions Condition { get; set; }
}
