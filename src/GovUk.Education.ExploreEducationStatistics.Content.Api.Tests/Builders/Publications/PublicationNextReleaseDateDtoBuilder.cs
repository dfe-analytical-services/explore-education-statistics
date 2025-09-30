using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationNextReleaseDateDtoBuilder
{
    private int _year = 2025;
    private int? _month = 9;
    private int? _day = 5;

    public PublicationNextReleaseDateDto Build() =>
        new()
        {
            Year = _year,
            Month = _month,
            Day = _day,
        };

    public PublicationNextReleaseDateDtoBuilder WithYear(int year)
    {
        _year = year;
        return this;
    }

    public PublicationNextReleaseDateDtoBuilder WithMonth(int? month)
    {
        _month = month;
        return this;
    }

    public PublicationNextReleaseDateDtoBuilder WithDay(int? day)
    {
        _day = day;
        return this;
    }
}
