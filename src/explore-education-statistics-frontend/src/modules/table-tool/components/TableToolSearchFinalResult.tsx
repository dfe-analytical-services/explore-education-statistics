import LoadingSpinner from '@common/components/LoadingSpinner';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import Link from '@frontend/components/Link';
import { FinalDataset } from '@frontend/services/tableToolSearchService';
import { useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';

interface TableToolSearchFinalResultProps {
  dataset: FinalDataset;
  releaseVersionId: string;
}

const TableToolSearchFinalResult = ({
  dataset,
  releaseVersionId,
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
          subjectId: '10308fbb-da53-4eae-20d2-08dec542d092',
          locationIds: [
            'dd13fe4c-d79d-4412-778c-08dec542d100',
            '3bbe6385-e5fc-4867-77d7-08dec542d100',
            'bd0133ed-6e3a-4f15-77ce-08dec542d100',
            '01c13f50-725d-4e33-77ca-08dec542d100',
          ],
          timePeriod: {
            startYear: 2014,
            startCode: 'AY',
            endYear: 2016,
            endCode: 'AY',
          },
          filters: [
            '04739429-a265-4f28-80a5-4a6fc96bc29e',
            'f6968c07-3256-41e9-a420-c6a35d78eaa9',
            'e5936411-6c33-46e4-b247-5d0a8059835f',
            '24b99a48-5448-4aba-a7c4-a1408cbbc1af',
          ],
          indicators: [
            '6543f18b-c9fd-4866-776e-08dec542d100',
            'dfbc7a76-1a0a-4649-7775-08dec542d100',
            '32a616ef-6ade-4514-7781-08dec542d100',
            'f2bab6fb-38c5-47f2-776d-08dec542d100',
            'd5034dd2-8b52-4a84-777d-08dec542d100',
            '370436e9-5880-444d-777f-08dec542d100',
          ],
        };
  const { data, isError, isLoading } = useQuery({
    ...tableBuilderQueries.getFullTable(fullTableQuery, releaseVersionId),
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
      <Link to={`/data-catalogue/${dataset.fileId}`}>View this data set</Link>
      <h3 className="govuk-heading-s govuk-!-margin-top-4">Relevance</h3>
      <p className="govuk-body">{dataset.aiSummary}</p>

      <LoadingSpinner loading={isLoading} className="govuk-!-margin-top-4">
        {isError && <p>Error loading table.</p>}
        {table && tableHeaders && (
          <TimePeriodDataTable
            capMaxHeight
            captionTitle={generatedCaption}
            defaultCaptionId={`dataTableCaption-${dataset.fileId}`}
            defaultFootnotesId={`dataTableFootnotes-${dataset.fileId}`}
            fullTable={table}
            query={fullTableQuery}
            releaseVersionId={releaseVersionId}
            tableHeadersConfig={tableHeaders}
          />
        )}
      </LoadingSpinner>
    </li>
  );
};

export default TableToolSearchFinalResult;
