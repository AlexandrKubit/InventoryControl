namespace Infrastructure.Entities;

using Infrastructure.Base;
using System.ComponentModel.DataAnnotations;

public class Receipt : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public string Number { get; set; }
    public DateTime Date { get; set; }
}
