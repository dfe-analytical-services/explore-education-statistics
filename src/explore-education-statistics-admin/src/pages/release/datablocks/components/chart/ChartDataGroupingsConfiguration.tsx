import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { MapDataGroupingConfig } from '@admin/pages/release/datablocks/components/chart/types/mapConfig';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import Effect from '@common/components/Effect';
import { MapConfig } from '@common/modules/charts/types/chart';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import React, { ReactNode } from 'react';
import ChartDataGroupingsConfigurationRow from './ChartDataGroupingsConfigurationRow';

const formId = 'chartDataGroupingsConfigurationForm';

interface Props {
  buttons?: ReactNode;
  map?: MapConfig;
  meta: FullTableMeta;
  onChange: (values: MapDataGroupingConfig) => void;
  onSubmit: (values: MapDataGroupingConfig) => void;
}

export default function ChartDataGroupingsConfiguration({
  buttons,
  map,
  meta,
  onChange,
  onSubmit,
}: Props) {
  const { forms, updateForm, submitForms } = useChartBuilderFormsContext();

  if (!map?.dataSetConfigs.length) {
    return <p>No data groupings to edit.</p>;
  }

  if (map.categoricalDataConfig?.length) {
    return <p>Data groupings cannot be set for categorical data.</p>;
  }

  return (
    <>
      <Effect
        value={{
          formKey: 'dataGroupings',
          isValid: true,
          submitCount: 0,
        }}
        onChange={updateForm}
        onMount={updateForm}
      />
      <table data-testid="chart-data-groupings">
        <thead>
          <tr>
            <th>Data set</th>
            <th>Groupings</th>
            <th className="govuk-!-text-align-right">Actions</th>
          </tr>
        </thead>
        <tbody>
          {map.dataSetConfigs.map(dataSetConfig => {
            const expandedDataSet = expandDataSet(dataSetConfig.dataSet, meta);
            const label = generateDataSetLabel(expandedDataSet);
            const key = generateDataSetKey(dataSetConfig.dataSet);
            const { unit } = expandedDataSet.indicator;

            return (
              <ChartDataGroupingsConfigurationRow
                dataSetConfig={dataSetConfig}
                key={key}
                label={label}
                map={map}
                meta={meta}
                unit={unit}
                onChange={onChange}
              />
            );
          })}
        </tbody>
      </table>

      <ChartBuilderSaveActions
        formId={formId}
        formKey="dataGroupings"
        onClick={async () => {
          updateForm({
            formKey: 'dataGroupings',
            submitCount: forms.dataGroupings
              ? forms.dataGroupings.submitCount + 1
              : 1,
          });

          onSubmit({
            dataSetConfigs: map.dataSetConfigs,
          });

          await submitForms();
        }}
      >
        {buttons}
      </ChartBuilderSaveActions>
    </>
  );
}
