import ChartBoundaryLevelsForm, {
  ChartBoundaryLevelsFormValues,
} from '@admin/pages/release/datablocks/components/chart/ChartBoundaryLevelsForm';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import {
  AxisConfiguration,
  MapConfig,
} from '@common/modules/charts/types/chart';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import parseNumber from '@common/utils/number/parseNumber';
import { isEqual } from 'lodash';
import React, { ReactNode, useCallback, useMemo } from 'react';
import generateDataSetLabel from './utils/generateDataSetLabel';

interface Props {
  buttons?: ReactNode;
  axisMajor: AxisConfiguration;
  data: TableDataResult[];
  legend: LegendConfiguration;
  map?: MapConfig;
  meta: FullTableMeta;
  options: ChartOptions;
  onChange: (values: Partial<ChartBoundaryLevelsFormValues>) => void;
  onSubmit: (values: ChartBoundaryLevelsFormValues) => void;
}

export default function ChartBoundaryLevelsConfiguration({
  buttons,
  axisMajor,
  data,
  legend,
  map,
  meta,
  options,
  onChange,
  onSubmit,
}: Props) {
  const normalizeValues = useCallback(
    (
      values: Partial<ChartBoundaryLevelsFormValues>,
    ): ChartBoundaryLevelsFormValues => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      return {
        boundaryLevel: values.boundaryLevel
          ? parseNumber(values.boundaryLevel)
          : undefined,
        dataSetConfigs:
          values.dataSetConfigs?.map(({ boundaryLevel, dataSet }) => ({
            boundaryLevel: parseNumber(boundaryLevel),
            dataSet,
          })) ?? [],
      };
    },
    [],
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

  const { dataSetConfigs, boundaryLevel } =
    useMemo<ChartBoundaryLevelsFormValues>(() => {
      const dataSetCategories = createDataSetCategories({
        axisConfiguration: {
          ...axisMajor,
          groupBy: 'locations',
        },
        data,
        meta,
      });

      const dataSetCategoryConfigs = getDataSetCategoryConfigs({
        dataSetCategories,
        legendItems: legend.items,
        meta,
        deprecatedDataClassification: options.dataClassification,
        deprecatedDataGroups: options.dataGroups,
      });

      return {
        boundaryLevel: options.boundaryLevel,
        dataSetConfigs: dataSetCategoryConfigs.map(({ rawDataSet }) => ({
          dataSet: rawDataSet,
          boundaryLevel: map?.dataSetConfigs.find(config =>
            isEqual(config.dataSet, rawDataSet),
          )?.boundaryLevel,
        })),
      };
    }, [axisMajor, data, meta, legend.items, map, options]);

  const mappedDataSetConfigs = useMemo(() => {
    return Object.values(dataSetConfigs).map(dataSetConfig => {
      const expandedDataSet = expandDataSet(dataSetConfig.dataSet, meta);
      const label = generateDataSetLabel(expandedDataSet);
      const key = generateDataSetKey(dataSetConfig.dataSet);

      return {
        label,
        key,
      };
    });
  }, [meta, dataSetConfigs]);

  return (
    <ChartBoundaryLevelsForm
      hasDataSetBoundaryLevels={false}
      buttons={buttons}
      boundaryLevelOptions={meta.boundaryLevels.map(({ id, label }) => ({
        label,
        value: id,
      }))}
      dataSetRows={mappedDataSetConfigs}
      initialValues={{ boundaryLevel, dataSetConfigs }}
      onChange={handleChange}
      onSubmit={handleSubmit}
    />
  );
}
