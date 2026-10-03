using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class DataSource : AggregateRoot<Guid>
{
    private readonly List<EntitySourceLink> _sourceLinks = [];
    private readonly List<SourceAssertion> _sourceAssertions = [];
    private readonly List<FeedItem> _feedItems = [];
    private readonly ReadOnlyCollection<EntitySourceLink> _sourceLinkView;
    private readonly ReadOnlyCollection<SourceAssertion> _sourceAssertionView;
    private readonly ReadOnlyCollection<FeedItem> _feedItemView;

    public Code ProviderCode { get; }
    public string DisplayName { get; private set; }
    public Code SourceType { get; }
    public Uri? Website { get; private set; }
    public SourceLicenseMetadata License { get; private set; }
    public decimal DefaultConfidenceWeight { get; private set; }
    public bool IsEnabled { get; private set; }
    public IReadOnlyCollection<EntitySourceLink> SourceLinks => _sourceLinkView;
    public IReadOnlyCollection<SourceAssertion> SourceAssertions => _sourceAssertionView;
    public IReadOnlyCollection<FeedItem> FeedItems => _feedItemView;

    public DataSource(
        Guid id,
        Code providerCode,
        string displayName,
        Code sourceType,
        Uri? website,
        SourceLicenseMetadata license,
        decimal defaultConfidenceWeight,
        bool isEnabled) : base(id)
    {
        ProviderCode = providerCode;
        DisplayName = displayName;
        SourceType = sourceType;
        Website = website;
        License = license;
        DefaultConfidenceWeight = defaultConfidenceWeight;
        IsEnabled = isEnabled;
        _sourceLinkView = _sourceLinks.AsReadOnly();
        _sourceAssertionView = _sourceAssertions.AsReadOnly();
        _feedItemView = _feedItems.AsReadOnly();
    }
}
