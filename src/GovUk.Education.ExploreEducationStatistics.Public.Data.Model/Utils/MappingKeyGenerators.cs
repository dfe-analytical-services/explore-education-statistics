namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;

public static class MappingKeyGenerators
{
    public static Func<LocationOptionMetaRow, string> LocationOptionMetaRow =>
        option => $"{option.Label} :: {option.GetRowKeyPretty()}";

    public static Func<LocationOptionMeta, string> LocationOptionMeta =>
        option => LocationOptionMetaRow(option.ToRow());

    public static Func<LocationOptionMetaLink, string> LocationOptionMetaLink =>
        link => LocationOptionMeta(link.Option);

    public static Func<FilterMeta, string> Filter => filter => filter.Column;

    public static Func<FilterOptionMeta, string> FilterOptionMeta => option => option.Label;

    public static Func<FilterOptionMetaLink, string> FilterOptionMetaLink =>
        link => FilterOptionMeta(link.Option);
}
