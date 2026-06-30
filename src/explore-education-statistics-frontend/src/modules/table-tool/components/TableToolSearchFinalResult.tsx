import LoadingSpinner from '@common/components/LoadingSpinner';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { FullTableQuery } from '@common/services/tableBuilderService';
import Link from '@frontend/components/Link';
import { FinalDataset } from '@frontend/services/tableToolSearchService';
import { useQuery } from '@tanstack/react-query';

interface TableToolSearchFinalResultProps {
  dataset: FinalDataset;
  releaseVersionId: string;
  testQuery: FullTableQuery;
}

const TableToolSearchFinalResult = ({
  dataset,
  releaseVersionId,
  testQuery,
}: TableToolSearchFinalResultProps) => {
  const { data, isError, isLoading } = useQuery({
    ...tableBuilderQueries.getFullTable(testQuery, releaseVersionId),
    refetchOnWindowFocus: false,
    staleTime: Infinity,
  });

  const { table, tableHeaders } = data ?? {};

  return (
    <li
      key={dataset.fileId}
      id={`result-${dataset.fileId}`}
      className="govuk-!-margin-bottom-8 govuk-!-padding-bottom-6 dfe-border-bottom"
    >
      <h2 className="govuk-heading-m govuk-!-margin-bottom-2">
        {dataset.title}
      </h2>
      <Link to={`/data-catalogue/${dataset.fileId}`}>View this data set</Link>
      <h3 className="govuk-heading-s govuk-!-margin-top-4">Relevance</h3>
      <p className="govuk-body">{dataset.aiSummary}</p>

      <LoadingSpinner loading={isLoading} className="govuk-!-margin-top-4">
        {isError && <p>Error loading table.</p>}
        {table && tableHeaders && (
          <TimePeriodDataTable
            capMaxHeight
            defaultCaptionId={`dataTableCaption-${dataset.fileId}`}
            defaultFootnotesId={`dataTableFootnotes-${dataset.fileId}`}
            fullTable={table}
            query={testQuery}
            releaseVersionId={releaseVersionId}
            tableHeadersConfig={tableHeaders}
          />
        )}
      </LoadingSpinner>
    </li>
  );
};

export default TableToolSearchFinalResult;
