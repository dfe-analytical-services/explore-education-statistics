import {
  AsyncState,
  AsyncStateSetterParam,
} from '@common/hooks/useAsyncCallback';
import { AsyncRetryState } from '@common/hooks/useAsyncRetry';
import useTableQuery from '@common/modules/find-statistics/hooks/useTableQuery';
import formatPretty from '@common/utils/number/formatPretty';
import { useCallback, useEffect, useMemo, useState } from 'react';

interface KeyStatResult {
  title: string;
  value: string;
}

export default function useKeyStatQuery(
  releaseId: string,
  dataBlockId: string,
): AsyncRetryState<KeyStatResult> {
  const {
    value: tableData,
    setState: setTableQueryState,
    ...tableQueryState
  } = useTableQuery(releaseId, dataBlockId);

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
