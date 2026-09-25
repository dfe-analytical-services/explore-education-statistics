import ErrorMessage from '@common/components/ErrorMessage';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
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
  const { timePeriod, filters, indicators, geographicLevels, subjectId } =
    dataset;

  return {
    subjectId,
    locationIds: Object.values(geographicLevels).flatMap(locations =>
      locations.map(location => location.id),
    ),
    timePeriod:
      timePeriod?.start?.year && timePeriod?.end?.year
        ? {
            startYear: timePeriod.start.year,
            startCode: timePeriod.start.code,
            endYear: timePeriod.end.year,
            endCode: timePeriod.end.code,
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
  const { isValidForTableGeneration, validationErrors, validationWarnings } =
    dataset;

  const fullTableQuery = generateQueryFromResult(dataset);

  const {
    data,
    isError,
    isInitialLoading: isLoading,
  } = useQuery({
    ...tableBuilderQueries.getFullTable(
      fullTableQuery,
      releaseVersionSummary.id,
    ),
    enabled: isValidForTableGeneration,
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
      <p className="govuk-body govuk-!-margin-top-4">
        {dataset.relevanceReason}
      </p>

      {isValidForTableGeneration ? (
        <>
          {validationWarnings.map(warning => (
            <WarningMessage key={`${warning.code}-${warning.message}`}>
              {warning.message}
            </WarningMessage>
          ))}

          <LoadingSpinner loading={isLoading} className="govuk-!-margin-top-4">
            {isError && (
              <ErrorMessage>Error loading table preview.</ErrorMessage>
            )}
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
        </>
      ) : (
        <>
          {validationErrors.length > 0 ? (
            validationErrors.map(error => (
              <ErrorMessage key={`${error.code}-${error.message}`}>
                {error.message}
              </ErrorMessage>
            ))
          ) : (
            <ErrorMessage>A table preview could not be generated.</ErrorMessage>
          )}
        </>
      )}
    </li>
  );
};

export default TableToolSearchFinalResult;
