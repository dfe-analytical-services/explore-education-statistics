namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;

public static class MappingKeyGenerators
{
    public static string LocationOptionMetaRow(LocationOptionMetaRow option) =>
        $"{option.Label} :: {option.GetRowKeyPretty()}";

    public static string LocationOptionMeta(LocationOptionMeta option) => LocationOptionMetaRow(option.ToRow());

    public static string LocationOptionMetaLink(LocationOptionMetaLink link) => LocationOptionMeta(link.Option);

    public static string Filter(FilterMeta filter) => filter.Column;

    public static string FilterOptionMeta(FilterOptionMeta option) => option.Label;

    public static string FilterOptionMetaLink(FilterOptionMetaLink link) => FilterOptionMeta(link.Option);

    public static string IndicatorMeta(IndicatorMeta indicator) => indicator.Column;
}
