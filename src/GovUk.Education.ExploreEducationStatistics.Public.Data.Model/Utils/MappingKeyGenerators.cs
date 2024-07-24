namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;

public static class MappingKeyGenerators
{
    public static Func<LocationOptionMetaRow, string> LocationOptionMetaRow =>
        option => $"{option.Label} :: {option.GetRowKeyPretty()}";

    public static Func<LocationOptionMeta, string> LocationOptionMeta =>
        option => LocationOptionMetaRow(option.ToRow());

    public static Func<FilterMeta, string> Filter =>
        filter => filter.PublicId;

    public static Func<FilterOptionMeta, string> FilterOption =>
        option => option.Label;
}
