using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TrainingCenter.Api.Contracts;
using TrainingCenter.Api.Controllers;
using TrainingCenter.Api.Models;
using TrainingCenter.Api.Services;
using TrainingCenter.Api.Services.Discounts;

namespace TrainingCenter.Tests;

public sealed class BusinessTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);
    private readonly CartPricingService pricing = new(new FixedTimeProvider(Now),
        [new GraduateDiscountRule(), new HotGroupDiscountRule()]);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AdditionalRuleIsUsedRegardlessOfRegistrationOrder(bool reverseOrder)
    {
        IDiscountRule[] rules = [new GraduateDiscountRule(), new HotGroupDiscountRule(), new FixedDiscountRule()];
        if (reverseOrder)
            Array.Reverse(rules);
        var calculator = new CartPricingService(new FixedTimeProvider(Now), rules);

        Assert.Equal(60m, calculator.CalculateTotal([new("C#", 100m, Now.AddDays(1))], true));
        var noRules = new CartPricingService(new FixedTimeProvider(Now), []);
        Assert.Equal(100m, noRules.CalculateTotal([new("C#", 100m, Now.AddDays(1))], true));
    }

    [Fact]
    public void GraduateGetsMaximumDiscountForEachCourse()
    {
        Course[] courses = [new("C#", 1000m, Now.AddDays(2)), new("SQL", 2000m, Now.AddDays(10))];
        Assert.Equal(2100m, pricing.CalculateTotal(courses, true));
        Assert.Equal(2800m, pricing.CalculateTotal(courses, false));
        Assert.Equal(0m, pricing.CalculateTotal([], false));
    }

    [Theory]
    [InlineData(-1, 100)]
    [InlineData(0, 100)]
    [InlineData(1, 80)]
    [InlineData(119.999, 80)]
    [InlineData(120, 100)]
    [InlineData(121, 100)]
    public void HotDiscountRespectsStrictTimeBoundary(double hours, int expected)
    {
        Assert.Equal((decimal)expected, pricing.CalculateTotal([new("C#", 100m, Now.AddHours(hours))], false));
    }

    [Theory]
    [InlineData(ClientType.Individual, "specialist.ufl@yandex.ru")]
    [InlineData(ClientType.Corporate, "specialist.corp@yandex.ru")]
    public async Task PrimaryFailureUsesCorrectBackupAndReturnsOk(ClientType type, string expectedAddress)
    {
        var sender = new RecordingBackupSender();
        var service = new BackupLeadService(sender, Options.Create(new EmailOptions
        {
            CorporateAddress = "specialist.corp@yandex.ru", IndividualAddress = "specialist.ufl@yandex.ru"
        }));
        var controller = new LeadsController(new UnavailablePrimaryEmailSender(), service, NullLogger<LeadsController>.Instance);
        var request = new CreateLeadRequest
        {
            Name = "Иван", Phone = "+7 999 123-45-67", Email = "ivan@example.com", ClientType = type
        };
        using var cancellation = new CancellationTokenSource();
        Assert.IsType<OkObjectResult>(await controller.Create(request, cancellation.Token));
        Assert.Equal(expectedAddress, sender.Recipient);
        Assert.Same(request, sender.Lead);
        Assert.Equal(1, sender.Calls);
        Assert.Equal(cancellation.Token, sender.Token);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FixedDiscountRule : IDiscountRule
    {
        public decimal GetDiscount(Course course, bool isGraduate, DateTimeOffset now) => 0.40m;
    }

    private sealed class RecordingBackupSender : IBackupEmailSender
    {
        public string? Recipient { get; private set; }
        public CreateLeadRequest? Lead { get; private set; }
        public int Calls { get; private set; }
        public CancellationToken Token { get; private set; }
        public Task SendAsync(string recipient, CreateLeadRequest lead, CancellationToken cancellationToken)
        {
            Recipient = recipient;
            Lead = lead;
            Token = cancellationToken;
            Calls++;
            return Task.CompletedTask;
        }
    }
}
