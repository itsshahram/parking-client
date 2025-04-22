using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Card;

public class CardCreditHistoryModel
{
    public Guid Id { get; set; }
    public int CardId { get; set; }
    public long CardSerialNo { get; set; }
    //Type: 1: Increase, 2: Decrease
    public byte Type { get; set; }
    public string? TypeName { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime CreateDate { get; set; }
}
