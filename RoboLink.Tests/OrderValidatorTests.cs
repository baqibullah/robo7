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


    [Fact]
    public void Empty_name_is_rejected()
    {
        var errors = OrderValidator.ValidateOrder(Order(""), "Pediatrics ward", "A12345678901A");
        Assert.Contains(errors, e => e.Field == "Patient name");
    }

    [Fact]
    public void Name_over_20_bytes_is_rejected()
    {
        var errors = OrderValidator.ValidateOrder(Order(new string('A', 21)), "Pediatrics ward", "A12345678901A");
        Assert.Contains(errors, e => e.Field == "Patient name");
    }

    [Fact]
    public void Non_english_name_counts_bytes_not_characters()
    {
        // Non-english 11 characters: é
        var errors = OrderValidator.ValidateOrder(Order(new string('é', 11)), "Pediatrics ward", "A12345678901A");
        Assert.Contains(errors, e => e.Field == "Patient name");
    }

    [Fact]
    public void Tray_number_over_9999_is_rejected()
    {
        var order = Order() with { TrayNo = 124345 };
        var errors = OrderValidator.ValidateOrder(order, "Pediatrics ward", "A12345678901A");
        Assert.Contains(errors, e => e.Field == "Tray number");
    }

    [Fact]
    public void Blank_queue_number_is_valid()
    {
        var order = Order() with { QueueNo = null };
        var errors = OrderValidator.ValidateOrder(order, "Pediatrics ward", "A12345678901A");
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(98)]
    public void Bad_stocker_code_is_rejected(int code)
    {
        var order = Order() with { StockerCode = code };
        var errors = OrderValidator.ValidateOrder(order, "Pediatrics ward", "A12345678901A");
        Assert.Contains(errors, e => e.Field == "Stocker code");
    }

    [Theory]
    [InlineData("A/B")]
    [InlineData("A:B")]
    [InlineData("A*B")]
    [InlineData("A?B")]
    public void Windows_file_name_character_in_name_is_rejected(string name)
    {
        var errors = OrderValidator.ValidateOrder(Order(name), "Pediatrics ward", "A12345678901A");
        Assert.Contains(errors, e => e.Field == "Patient name");
    }
}
