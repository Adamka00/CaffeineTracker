using Caffeine.Models;
using Caffeine.Services;
using Xunit;

namespace Caffeine.Tests;

public sealed class PlanningInsightTests
{
    private readonly StandardCaffeineDecayStrategy decay = new();

    private CaffeinePlanningService Planning =>
        new(new CaffeineCalculatorService(decay), decay);

    [Fact]
    public void CutoffUsesExistingHalfLifeAndNeverChoosesTheAbsorptionLoophole()
    {
        var bed =
            new DateTime(
                2026,
                9,
                17,
                23,
                0,
                0);

        var cutoff =
            Planning.LatestIntake(
                [],
                bed,
                80,
                20);

        Assert.NotNull(cutoff);

        Assert.InRange(
            (
                cutoff.Value -
                new DateTime(
                    2026,
                    9,
                    17,
                    12,
                    15,
                    0)
            ).TotalMinutes,
            -1,
            0);

        Assert.True(
            decay.CalculateActiveCaffeine(
                80,
                cutoff.Value,
                bed) <= 20);

        Assert.Equal(
            bed.AddMinutes(-45),
            Planning.LatestIntake(
                [],
                bed,
                10,
                25));

        Assert.Equal(
            bed,
            Planning.LatestIntake(
                [],
                bed,
                0,
                25));
    }

    [Fact]
    public void ExistingCaffeineCanMakeTargetImpossibleAndForecastIncludesBothCurves()
    {
        var now =
            new DateTime(
                2026,
                9,
                17,
                14,
                0,
                0);

        CaffeineLog[] logs =
        [
            new()
            {
                ConsumedAt =
                    now.AddHours(-1),

                TotalCaffeineMg =
                    160
            }
        ];

        Assert.Null(
            Planning.LatestIntake(
                logs,
                now.AddHours(1),
                80,
                25));

        var forecast =
            Planning.Calculate(
                logs,
                now,
                now.AddHours(9),
                now,
                80,
                25);

        Assert.True(
            forecast.WithDrinkAtBedtime >
            forecast.ExistingAtBedtime);

        Assert.True(
            forecast.UnderTargetAt >
            now.AddMinutes(45));

        Assert.Equal(
            forecast.Curve[0].ExistingMg,
            forecast.Curve[0].WithDrinkMg);

        Assert.True(
            forecast.Curve[2].WithDrinkMg >
            forecast.Curve[2].ExistingMg);
    }

    [Fact]
    public void TargetCrossingWaitsForPeakEvenIfCurrentEstimateIsZero()
    {
        var now =
            new DateTime(
                2026,
                9,
                17,
                14,
                0,
                0);

        var result =
            Planning.UnderTarget(
                [
                    new()
                    {
                        ConsumedAt = now,
                        TotalCaffeineMg = 160
                    }
                ],
                now,
                25);

        Assert.True(
            result >
            now.AddHours(10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(9)]
    public void InsightsRequireTenUsableNights(
        int count)
    {
        var logs =
            Enumerable
                .Range(0, count)
                .Select(
                    i =>
                        new SleepLog
                        {
                            SleepDate =
                                new DateTime(
                                    2026,
                                    9,
                                    1)
                                .AddDays(i),

                            SleepRating = 3,

                            ActualBedtime =
                                new DateTime(
                                    2026,
                                    9,
                                    1),

                            WakeTime =
                                new DateTime(
                                    2026,
                                    9,
                                    1,
                                    8,
                                    0,
                                    0)
                        });

        Assert.All(
            new SleepInsightService()
                .Analyze(logs),
            i =>
                Assert.Equal(
                    "InsufficientSleepData",
                    i.MessageKey));
    }

    [Fact]
    public void InsightsAreDeterministicHandleNoVariationAndMissingOptionalData()
    {
        var logs =
            Enumerable
                .Range(0, 10)
                .Select(
                    i =>
                        new SleepLog
                        {
                            SleepDate =
                                new DateTime(
                                    2026,
                                    9,
                                    1)
                                .AddDays(i),

                            SleepRating =
                                5 - i / 2,

                            EstimatedCaffeineAtBedtime =
                                i * 20,

                            PreviousDayCaffeineMg =
                                100,

                            ActualBedtime =
                                new DateTime(
                                    2026,
                                    9,
                                    1),

                            WakeTime =
                                new DateTime(
                                    2026,
                                    9,
                                    1,
                                    8,
                                    0,
                                    0)
                        })
                .ToList();

        var actual =
            new SleepInsightService()
                .Analyze(logs);

        Assert.Equal(
            "SleepLowerAssociation",
            actual[0].MessageKey);

        Assert.Equal(
            "SleepNoVariation",
            actual[1].MessageKey);

        Assert.Equal(
            "InsufficientSleepData",
            actual[2].MessageKey);

        Assert.Equal(
            actual,
            new SleepInsightService()
                .Analyze(logs));
    }
}