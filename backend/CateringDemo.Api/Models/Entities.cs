namespace CateringDemo.Api.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public double QuantityOnHand { get; set; }
    public double ReorderLevel { get; set; }
    public decimal CostPerUnit { get; set; }
}

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal PricePerServing { get; set; }
    public List<RecipeLine> Recipe { get; set; } = new();
}

// How much of one ingredient a single serving of a menu item uses.
public class RecipeLine
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public double QuantityPerServing { get; set; }
}

public class CateringOrder
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public int Headcount { get; set; }
    public string Status { get; set; } = "Pending";
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderLine
{
    public int Id { get; set; }
    public int CateringOrderId { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int Servings { get; set; }
}
