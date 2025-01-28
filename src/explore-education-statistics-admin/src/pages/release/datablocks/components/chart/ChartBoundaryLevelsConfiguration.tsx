import ChartBoundaryLevelsForm, {
  ChartBoundaryLevelsFormValues,
} from '@admin/pages/release/datablocks/components/chart/ChartBoundaryLevelsForm';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { MapBoundaryLevelConfig } from '@admin/pages/release/datablocks/components/chart/types/mapConfig';
import { MapConfig } from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import parseNumber from '@common/utils/number/parseNumber';
import React, { ReactNode, useCallback, useMemo } from 'react';

interface Props {
  buttons?: ReactNode;
  map: MapConfig;
  meta: FullTableMeta;
  options: ChartOptions;
  onChange: (values: MapBoundaryLevelConfig) => void;
  onSubmit: (values: MapBoundaryLevelConfig) => void;
}

export default function ChartBoundaryLevelsConfiguration({
  buttons,
  map,
  meta,
  options,
  onChange,
  onSubmit,
}: Props) {
  const initialValues = useMemo<ChartBoundaryLevelsFormValues>(() => {
    return {
      boundaryLevel: options.boundaryLevel?.toString(),
      dataSetConfigs:
        map?.dataSetConfigs.map(dataSetConfig => {
          return {
            boundaryLevel: dataSetConfig.boundaryLevel?.toString(),
          };
        }) ?? [],
    };
  }, [options.boundaryLevel, map?.dataSetConfigs]);

  const normalizeValues = useCallback(
    (
      values: Partial<ChartBoundaryLevelsFormValues>,
    ): MapBoundaryLevelConfig => {
      return {
        boundaryLevel: parseNumber(values.boundaryLevel),
        dataSetConfigs:
          values.dataSetConfigs?.map(({ boundaryLevel }, index) => {
            return {
              boundaryLevel: parseNumber(boundaryLevel),
              dataSet: map.dataSetConfigs[index].dataSet,
            };
          }) ?? [],
      };
    },
    [map.dataSetConfigs],
  );

  const handleSubmit = useCallback(
    (values: ChartBoundaryLevelsFormValues) => {
      onSubmit(normalizeValues(values));
    },
    [onSubmit, normalizeValues],
  );

  const handleChange = useCallback(
    (values: Partial<ChartBoundaryLevelsFormValues>) => {
      onChange(normalizeValues(values));
    },
    [onChange, normalizeValues],
  );

  return (
    <ChartBoundaryLevelsForm
      buttons={buttons}
      dataSetConfigs={map.dataSetConfigs}
      initialValues={initialValues}
      meta={meta}
      onChange={handleChange}
      onSubmit={handleSubmit}
    />
  );
}
