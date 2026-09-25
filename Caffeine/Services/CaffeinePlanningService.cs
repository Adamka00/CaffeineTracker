using Caffeine.Models;

namespace Caffeine.Services;

public sealed record PlanningPoint(
    DateTime At,
    double ExistingMg,
    double WithDrinkMg);

public sealed record PlanningResult(
    double DoseMg,
    double ExistingAtBedtime,
    double WithDrinkAtBedtime,
    DateTime? Cutoff,
    DateTime? UnderTargetAt,
    IReadOnlyList<PlanningPoint> Curve);

public sealed class CaffeinePlanningService(
    ICaffeineCalculatorService calculator,
    ICaffeineDecayStrategy decay)
{
    public DateTime? LatestIntake(
        IEnumerable<CaffeineLog> logs,
        DateTime bedtime,
        double dose,
        double target)
    {
        if (!double.IsFinite(dose) ||
            dose < 0 ||
            !double.IsFinite(target) ||
            target <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dose));
        }

        var remaining =
            target -
            calculator.GetCurrentTotalActiveCaffeine(
                logs,
                bedtime);

        if (remaining < 0 ||
            (remaining == 0 && dose > 0))
        {
            return null;
        }

        if (dose == 0)
            return bedtime;

        // Only the descending branch is valid:
        // intake at bedtime must not look like zero exposure.
        var latest =
            bedtime - decay.TimeToPeak;

        if (decay.CalculateActiveCaffeine(
                dose,
                latest,
                bedtime) <= remaining)
        {
            return latest;
        }

        var earliest =
            bedtime.AddDays(-14);

        if (decay.CalculateActiveCaffeine(
                dose,
                earliest,
                bedtime) > remaining)
        {
            return null;
        }

        while ((latest - earliest).TotalSeconds > 1)
        {
            var middle =
                earliest.AddTicks(
                    (latest - earliest).Ticks / 2);

            if (decay.CalculateActiveCaffeine(
                    dose,
                    middle,
                    bedtime) <= remaining)
            {
                earliest = middle;
            }
            else
            {
                latest = middle;
            }
        }

        // Round toward earlier consumption,
        // never falsely under the target.
        return new DateTime(
            earliest.Ticks -
            earliest.Ticks % TimeSpan.TicksPerMinute);
    }

    public DateTime? UnderTarget(
        IEnumerable<CaffeineLog> source,
        DateTime at,
        double target)
    {
        var logs = source
            .Where(l => l.TotalCaffeineMg > 0)
            .ToArray();

        var start = logs.Length == 0
            ? at
            : new[]
            {
                at,
                logs.Max(l => l.ConsumedAt)
                + decay.TimeToPeak
            }.Max();

        if (calculator
                .GetCurrentTotalActiveCaffeine(
                    logs,
                    start) <= target)
        {
            return start;
        }

        for (var t = start;
             t <= start.AddDays(14);
             t = t.AddMinutes(5))
        {
            if (calculator
                    .GetCurrentTotalActiveCaffeine(
                        logs,
                        t) <= target)
            {
                return t;
            }
        }

        return null;
    }

    public PlanningResult Calculate(
        IEnumerable<CaffeineLog> source,
        DateTime now,
        DateTime bedtime,
        DateTime intake,
        double dose,
        double target)
    {
        var logs = source.ToArray();

        var combined = logs
            .Append(new CaffeineLog
            {
                ConsumedAt = intake,
                TotalCaffeineMg = dose
            })
            .ToArray();

        var end = new[]
        {
            now.AddHours(24),
            bedtime.AddHours(6),
            intake.AddHours(24)
        }.Max();

        var points =
            new List<PlanningPoint>();

        for (var t = now;
             t <= end;
             t = t.AddMinutes(30))
        {
            points.Add(new(
                t,

                Math.Round(
                    calculator
                        .GetCurrentTotalActiveCaffeine(
                            logs,
                            t),
                    1),

                Math.Round(
                    calculator
                        .GetCurrentTotalActiveCaffeine(
                            combined,
                            t),
                    1)));
        }

        return new(
            dose,

            calculator
                .GetCurrentTotalActiveCaffeine(
                    logs,
                    bedtime),

            calculator
                .GetCurrentTotalActiveCaffeine(
                    combined,
                    bedtime),

            LatestIntake(
                logs,
                bedtime,
                dose,
                target),

            UnderTarget(
                combined,
                now,
                target),

            points);
    }
}