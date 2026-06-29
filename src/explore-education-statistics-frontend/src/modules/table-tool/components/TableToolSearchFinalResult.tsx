import LoadingSpinner from '@common/components/LoadingSpinner';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import tableBuilderQueries from '@common/queries/tableBuilderQueries';
import { FullTableQuery } from '@common/services/tableBuilderService';
import Link from '@frontend/components/Link';
import { FinalDataset } from '@frontend/services/tableToolSearchService';
import { useQuery } from '@tanstack/react-query';

interface TableToolSearchFinalResultProps {
  dataset: FinalDataset;
}

const TableToolSearchFinalResult = ({
  dataset,
}: TableToolSearchFinalResultProps) => {
  // Hardcoded query from seed data.
  // To swap out for dataset data later on.
  const query = {
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
  } as FullTableQuery;

  const { data, isError, isFetching, isLoading } = useQuery({
    ...tableBuilderQueries.getFullTable(query),
    keepPreviousData: true,
    refetchOnWindowFocus: false,
    staleTime: Infinity,
  });

  const { table, tableHeaders } = data ?? {};

  return (
    <li
      key={dataset.fileId}
      id={`result-${dataset.fileId}`}
      className="govuk-!-margin-bottom-4 govuk-!-padding-bottom-4"
    >
      <h2 className="govuk-heading-m govuk-!-margin-bottom-2">
        {dataset.title}
      </h2>
      <Link to={`/data-catalogue/${dataset.fileId}`}>View this data set</Link>
      <h3 className="govuk-heading-s govuk-!-margin-top-4">Relevance</h3>
      <p className="govuk-body">{dataset.aiSummary}</p>

      <LoadingSpinner
        loading={isLoading || isFetching}
        className="govuk-!-margin-top-4"
      >
        {isError && <p>Error loading table.</p>}
        {table && tableHeaders && (
          <TimePeriodDataTable
            footnotesClassName="govuk-!-width-two-thirds"
            fullTable={table}
            query={query}
            tableHeadersConfig={tableHeaders}
          />
        )}
      </LoadingSpinner>
    </li>
  );
};

export default TableToolSearchFinalResult;
