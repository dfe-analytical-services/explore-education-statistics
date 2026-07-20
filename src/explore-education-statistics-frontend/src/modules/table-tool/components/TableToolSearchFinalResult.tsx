import ErrorMessage from '@common/components/ErrorMessage';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { ReleaseVersionSummary } from '@common/services/publicationService';
import { FullTableQuery } from '@common/services/tableBuilderService';
import Link from '@frontend/components/Link';
import styles from '@frontend/modules/table-tool/components/TableToolSearchFinalResult.module.scss';
import { encodeFullTableQueryToParams } from '@frontend/modules/table-tool/utils/fullTableQueryTranscode';
import { FinalDataset } from '@frontend/services/tableToolSearchService';
import { useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';

interface TableToolSearchFinalResultProps {
  dataset: FinalDataset;
  releaseVersionSummary: ReleaseVersionSummary;
}

const generateQueryFromResult = (dataset: FinalDataset): FullTableQuery => {
  const {
    timePeriod: { start, end } = {},
    filters,
    indicators,
    geographicLevels,
    subjectId,
  } = dataset;
  return {
    subjectId,
    locationIds: Object.values(geographicLevels).flatMap(locations =>
      locations.map(location => location.id),
    ),
    timePeriod:
      start?.year && end?.year
        ? {
            startYear: parseInt(start.year, 10),
            startCode: start.code,
            endYear: parseInt(end.year, 10),
            endCode: end.code,
          }
        : undefined,
    filters: filters.map(filter => filter.id),
    indicators: indicators.map(indicator => indicator.id),
  };
};

const TableToolSearchFinalResult = ({
  dataset,
  releaseVersionSummary,
}: TableToolSearchFinalResultProps) => {
  const fullTableQuery = generateQueryFromResult(dataset);

  const { data, isError, isLoading } = useQuery({
    ...tableBuilderQueries.getFullTable(
      fullTableQuery,
      releaseVersionSummary.id,
    ),
    refetchOnWindowFocus: false,
    staleTime: Infinity,
  });

  const { table, tableHeaders } = data ?? {};

  const generatedCaption = useMemo<string>(
    () => (table?.subjectMeta ? generateTableTitle(table.subjectMeta) : ''),
    [table?.subjectMeta],
  );

  return (
    <li
      key={dataset.fileId}
      id={`result-${dataset.fileId}`}
      className="govuk-!-margin-bottom-8 govuk-!-padding-bottom-6 dfe-border-bottom"
    >
      <h2 className="govuk-heading-m govuk-!-margin-bottom-2">
        {dataset.title}
      </h2>
      <Link to={`/data-catalogue/data-set/${dataset.dataSetFileId}`}>
        View this data set <VisuallyHidden> - {dataset.title}</VisuallyHidden>
      </Link>
      <h3 className="govuk-heading-s govuk-!-margin-top-4">Relevance</h3>
      <p className="govuk-body">{dataset.relevanceReason}</p>

      <LoadingSpinner loading={isLoading} className="govuk-!-margin-top-4">
        {isError && <ErrorMessage>Error loading table preview.</ErrorMessage>}
        {table && tableHeaders && (
          <>
            <div className={styles.previewNotice}>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Table showing a preview from:
                <br />
                {dataset.title}
              </p>
              <Link
                to={`/data-tables/${releaseVersionSummary.publication.slug}/${
                  releaseVersionSummary.slug
                }?fromSearch&${encodeFullTableQueryToParams(fullTableQuery)}`}
              >
                View and edit this table{' '}
                <VisuallyHidden> - {dataset.title}</VisuallyHidden>
              </Link>
            </div>
            <TimePeriodDataTable
              capMaxHeight
              captionTitle={generatedCaption}
              defaultCaptionId={`dataTableCaption-${dataset.fileId}`}
              defaultFootnotesId={`dataTableFootnotes-${dataset.fileId}`}
              fullTable={table}
              query={fullTableQuery}
              releaseVersionId={releaseVersionSummary.id}
              tableHeadersConfig={tableHeaders}
            />
          </>
        )}
      </LoadingSpinner>
    </li>
  );
};

export default TableToolSearchFinalResult;
