﻿namespace Infrastructure.Entities;

using Infrastructure.Base;
using System.ComponentModel.DataAnnotations;

public class ReceiptItem : IGuidIdentity
{
    [Key]
    public Guid Guid { get; set; }
    public Guid ReceiptGuid { get; set; }
    public Guid ResourceGuid { get; set; }
    public Guid MeasureUnitGuid { get; set; }
    public decimal Quantity { get; set; }
}
