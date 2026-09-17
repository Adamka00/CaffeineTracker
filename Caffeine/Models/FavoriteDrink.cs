namespace Caffeine.Models;

public class FavoriteDrink
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int BeverageId { get; set; }
    public Beverage Beverage { get; set; } = null!;
    public int AmountMl { get; set; }
}