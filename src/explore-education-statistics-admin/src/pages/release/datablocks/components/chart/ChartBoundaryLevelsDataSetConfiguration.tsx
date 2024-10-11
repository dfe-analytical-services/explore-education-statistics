import { FormFieldSelect } from '@common/components/form';
import { MapDataSetConfig } from '@common/modules/charts/types/chart';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import React, { useMemo } from 'react';
import generateDataSetLabel from './utils/generateDataSetLabel';

export default function ChartBoundaryLevelsDataSetConfiguration({
  dataSetConfigs,
  meta,
}: {
  dataSetConfigs: Omit<MapDataSetConfig, 'dataGrouping'>[];
  meta: FullTableMeta;
}) {
  const mappedDataSetConfigs = useMemo(() => {
    return dataSetConfigs.map(dataSetConfig => {
      const expandedDataSet = expandDataSet(dataSetConfig.dataSet, meta);
      const dataSetLabel = generateDataSetLabel(expandedDataSet);
      const key = generateDataSetKey(dataSetConfig.dataSet);

      return {
        dataSetLabel,
        key,
      };
    });
  }, [meta, dataSetConfigs]);
  return (
    <>
      <h4>Set boundary levels per data set</h4>
      {!!dataSetConfigs && dataSetConfigs.length > 1 && (
        <table data-testid="chart-boundary-level-selections">
          <thead>
            <tr>
              <th>Data set</th>
              <th>Boundary</th>
            </tr>
          </thead>
          <tbody>
            {mappedDataSetConfigs.map(({ dataSetLabel, key }, index) => {
              return (
                <tr key={key}>
                  <td>{dataSetLabel}</td>
                  <td>
                    <FormFieldSelect
                      label={`Boundary level for dataset: ${dataSetLabel}`}
                      hideLabel
                      name={`dataSetConfigs.${index}.boundaryLevel`}
                      order={[]}
                      options={[
                        {
                          label: 'Use default',
                          value: '',
                        },
                        ...meta.boundaryLevels.map(({ id, label }) => ({
                          value: Number(id),
                          label,
                        })),
                      ]}
                    />
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
    </>
  );
}
