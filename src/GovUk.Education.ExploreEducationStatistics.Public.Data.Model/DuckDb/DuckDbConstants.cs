namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;

public static class DuckDbConstants
{
    /// <summary>
    /// A standard set of options to inform DuckDB of our CSV formatting
    /// preferences and prevent assumptions made by early optimisation
    /// during its CSV-reading process. Without these explicitly stated, it
    /// can lead to issues where it has made its own assumptions as to
    /// which cell delimiters and other special characters it expects to
    /// see, only to then hit an exception to its own rule (like a
    /// quote-delimited cell with a comma inside) and cause a read failure.
    /// </summary>
    public const string ReadCsvOptions = """
        ALL_VARCHAR = true,
        QUOTE = '"',
        DELIM = ','
        """;
}
