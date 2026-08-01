using System.ComponentModel.DataAnnotations;

namespace StallBazar.Models;

public class Stall
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required, StringLength(30)]
    public string Number { get; set; } = string.Empty;

    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Tier { get; set; } = "Standard";

    [Required, StringLength(60)]
    public string Type { get; set; } = "Food";

    [StringLength(80)]
    public string Zone { get; set; } = "Main aisle";

    [Required, StringLength(40)]
    public string Size { get; set; } = string.Empty;

    [Range(0, 9999)]
    public decimal Length { get; set; } = 3;

    [Range(0, 9999)]
    public decimal Breadth { get; set; } = 3;

    [Range(0, 999999)]
    public decimal Price { get; set; }

    public StallStatus Status { get; set; } = StallStatus.Available;
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public List<Booking> Bookings { get; set; } = [];
}
