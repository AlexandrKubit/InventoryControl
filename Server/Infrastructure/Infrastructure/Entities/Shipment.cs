namespace Infrastructure.Entities;

using Infrastructure.Base;
using System.ComponentModel.DataAnnotations;
using static Domain.Entities.Warehouse.Shipment.Document;

public class Shipment : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public string Number { get; set; }
    public Guid ClientGuid { get; set; }
    public DateTime Date { get; set; }
    public Conditions Condition { get; set; }
}
