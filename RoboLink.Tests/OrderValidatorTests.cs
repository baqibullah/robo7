using RoboLink.Core;

namespace RoboLink.Tests;

public class OrderValidatorTests
{
    private static TubeOrder Order(string name = "Tiger Woords") =>
        new(5, 1, new DateOnly(2026, 11, 26), 32, false, 1, 1234,
        "0123456789", name, "HbA1c", 1.0m, "Pediatrics", 2, 99);


    [Fact]
    public void Valid_order_has_no_errors()
    {
        var errors = OrderValidator.ValidateOrder(Order(), "Pediatrics ward", "A12345678901A");
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("Woods, Tiger")]
    [InlineData("Tiger|Woods")]
    [InlineData("Tiger^Woods")]
    [InlineData("Tiger\rWoods")]
    public void Forbidden_character_in_name_is_rejected(string name)
    {
        var errors = OrderValidator.ValidateOrder(Order(name), "Pediatrics ward", "A12345678901A");
        Assert.Single(errors);
        Assert.Equal("Patient name", errors[0].Field);
    }

    [Fact]
    public void Names_like_Cruz_and_CRP_are_fine()
    {
        var errors = OrderValidator.ValidateOrder(Order("Cruz"), "Pediatrics ward", "A12345678901A");
        Assert.Empty(errors);
    }

}
