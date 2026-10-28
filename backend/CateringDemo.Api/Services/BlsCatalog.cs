namespace CateringDemo.Api.Services;

// BLS average price series used by the app. A series ID is
// "APU" + area code + item code, for example APU0300706111 is whole chicken in the South.
// Item codes come from the BLS average price publication list:
// https://www.bls.gov/cpi/additional-resources/average-price-publication-list.htm
public static class BlsCatalog
{
    public const string NationalRegion = "0000";

    public static readonly IReadOnlyDictionary<string, string> Regions = new Dictionary<string, string>
    {
        ["0000"] = "U.S. average",
        ["0100"] = "Northeast",
        ["0200"] = "Midwest",
        ["0300"] = "South",
        ["0400"] = "West",
    };

    // Every item below is priced per pound.
    public static readonly IReadOnlyDictionary<string, string> Items = new Dictionary<string, string>
    {
        ["706212"] = "Chicken legs, bone-in",
        ["703112"] = "Ground beef, 100% beef",
        ["701312"] = "Rice, white, long grain, uncooked",
        ["701322"] = "Spaghetti and macaroni",
        ["712311"] = "Tomatoes, field grown",
        ["712406"] = "Peppers, sweet",
        ["712112"] = "Potatoes, white",
        ["FL2101"] = "Lettuce, romaine",
        ["714233"] = "Beans, dried, any type",
    };

    public static string SeriesId(string region, string itemCode) => $"APU{region}{itemCode}";

    public static IEnumerable<string> AllSeriesIds() =>
        from region in Regions.Keys
        from item in Items.Keys
        select SeriesId(region, item);

    public static bool IsValidRegion(string? region) => region is not null && Regions.ContainsKey(region);
}
