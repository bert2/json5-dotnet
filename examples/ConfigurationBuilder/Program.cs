using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJson5File("appsettings.json5")
    .Build();

Console.WriteLine(
    $"""
    Raw config entries:

    {string.Join(Environment.NewLine, config.AsEnumerable())}

    Deserialized config:

    {config.Get<Config>()}
    """);

public record Config(string Description, double[] Nums) {
    public override string ToString()
        => $"Config {{ Description = {Description[..18]}..., Nums = [{string.Join(", ", Nums)}] }}";
}
