import ErrorMessage from '@common/components/ErrorMessage';
import LoadingSpinner from '@common/components/LoadingSpinner';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { ReleaseVersionSummary } from '@common/services/publicationService';
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

const TableToolSearchFinalResult = ({
  dataset,
  releaseVersionSummary,
}: TableToolSearchFinalResultProps) => {
  // Use test data for now, implement when backend provides required data
  const fullTableQuery =
    dataset.title === 'Test final result'
      ? {
          subjectId: '821750f6-939f-4f60-20d4-08dec542d092',
          locationIds: [
            'a455a027-e635-4e90-a0b8-08dec542d106',
            'a2857282-154f-44cb-a0bb-08dec542d106',
          ],
          timePeriod: {
            startYear: 2014,
            startCode: 'AY',
            endYear: 2016,
            endCode: 'AY',
          },
          filters: [],
          filterHierarchiesOptions: {},
          indicators: [
            '4f9f7d79-c3a5-459c-a0b5-08dec542d106',
            '1bd72149-5230-40ab-a0ac-08dec542d106',
            '0e74dcb9-9c90-4747-a0a4-08dec542d106',
            '922db259-ddcc-4a64-a09d-08dec542d106',
          ],
        }
      : {
          subjectId: '2dc0f701-dbe6-44bc-4772-08debbdc36fb',
          locationIds: ['376f9a26-dc39-4db3-bb19-0549e59d322a'],
          timePeriod: {
            startYear: 2024,
            startCode: 'AY',
            endYear: 2024,
            endCode: 'AY',
          },
          filters: [
            '9d5df94e-67b1-4535-a753-4fc7ba0e589b',
            '373dad8a-a916-464e-8824-c2c68691a4b3',
            '24784681-ee31-4cf9-a38e-096d679747b4',
            '82a5fc83-99f5-494e-b7be-97559cfa2c1c',
          ],
          filterHierarchiesOptions: {},
          indicators: [
            '1a01f5af-049b-4877-2fc6-08debbdc383d',
            '1ca177aa-4fca-4493-2fc3-08debbdc383d',
          ],
        };
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
      <Link to={`/data-catalogue/data-set/${dataset.fileId}`}>
        View this data set
      </Link>
      <h3 className="govuk-heading-s govuk-!-margin-top-4">Relevance</h3>
      <p className="govuk-body">{dataset.aiSummary}</p>

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
                View and edit this table
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
