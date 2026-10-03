using System.Collections.Frozen;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Routing;

public sealed record AccessibilitySummary
{
    public AccessibilitySummary(
        IEnumerable<Code> satisfiedHardConstraints,
        IEnumerable<Code> relevantUnknowns,
        IEnumerable<Code> importantFacts)
    {
        ArgumentNullException.ThrowIfNull(satisfiedHardConstraints);
        ArgumentNullException.ThrowIfNull(relevantUnknowns);
        ArgumentNullException.ThrowIfNull(importantFacts);
        SatisfiedHardConstraints = satisfiedHardConstraints.ToFrozenSet();
        RelevantUnknowns = relevantUnknowns.ToFrozenSet();
        ImportantFacts = importantFacts.ToFrozenSet();
    }

    public IReadOnlySet<Code> SatisfiedHardConstraints { get; }

    public IReadOnlySet<Code> RelevantUnknowns { get; }

    public IReadOnlySet<Code> ImportantFacts { get; }
}
