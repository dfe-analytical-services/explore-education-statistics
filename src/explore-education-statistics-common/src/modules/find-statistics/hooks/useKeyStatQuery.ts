import { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import useTableQuery, {
  TableQueryOptions,
} from '@common/modules/find-statistics/hooks/useTableQuery';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import formatPretty from '@common/utils/number/formatPretty';
import { useMemo } from 'react';

interface KeyStatResult {
  title: string;
  value: string;
}

export default function useKeyStatQuery(
  query: ReleaseTableDataQuery,
  options: TableQueryOptions = {},
): AsyncRetryState<KeyStatResult> {
  const { value: tableData, ...state } = useTableQuery(
    {
      ...query,
      includeGeoJson: false,
    },
    options,
  );

  const value = useMemo<KeyStatResult | undefined>(() => {
    if (tableData) {
      const [indicator] = tableData.subjectMeta.indicators;

      return {
        title: indicator.label,
        value: formatPretty(
          tableData.results[0].measures[indicator.value],
          indicator.unit,
          indicator.decimalPlaces,
        ),
      };
    }

    return undefined;
  }, [tableData]);

  return {
    value,
    ...state,
  };
}
