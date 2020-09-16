import {
  AsyncState,
  AsyncStateSetterParam,
} from '@common/hooks/useAsyncCallback';
import { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import useTableQuery, {
  TableQueryOptions,
} from '@common/modules/find-statistics/hooks/useTableQuery';
import { ReleaseTableDataQuery } from '@common/services/tableBuilderService';
import formatPretty from '@common/utils/number/formatPretty';
import { useCallback, useEffect, useMemo, useState } from 'react';

interface KeyStatResult {
  title: string;
  value: string;
}

export default function useKeyStatQuery(
  query: ReleaseTableDataQuery,
  options: TableQueryOptions = {},
): AsyncRetryState<KeyStatResult> {
  const {
    value: tableData,
    setState: setTableQueryState,
    ...tableQueryState
  } = useTableQuery(
    {
      ...query,
      includeGeoJson: false,
    },
    options,
  );

  const [value, setValue] = useState<KeyStatResult>();

  useEffect(() => {
    if (tableData) {
      const [indicator] = tableData.subjectMeta.indicators;

      if (!indicator) {
        return;
      }

      const indicatorValue = tableData.results[0]?.measures[indicator.value];

      if (!indicatorValue) {
        return;
      }

      setValue({
        title: indicator.label,
        value: formatPretty(
          indicatorValue,
          indicator.unit,
          indicator.decimalPlaces,
        ),
      });
    }
  }, [tableData]);

  const setState = useCallback(
    (state: AsyncStateSetterParam<KeyStatResult>) => {
      const typedState = state as AsyncState<KeyStatResult>;

      if (typeof typedState.value !== 'undefined') {
        setValue(typedState.value);
        setTableQueryState({ isLoading: false, value: tableData });
      } else {
        setTableQueryState(typedState);
      }
    },
    [setTableQueryState, tableData],
  );

  return useMemo(() => {
    return {
      ...tableQueryState,
      value,
      setState,
    };
  }, [setState, tableQueryState, value]);
}
