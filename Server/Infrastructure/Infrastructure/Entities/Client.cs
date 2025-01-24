namespace Infrastructure.Entities;

using Infrastructure.Base;
using System.ComponentModel.DataAnnotations;
using static Domain.Entities.Directories.Client;

public class Client : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public Conditions Condition { get; set; }
}
