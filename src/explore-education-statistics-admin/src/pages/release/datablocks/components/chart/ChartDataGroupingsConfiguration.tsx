import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import ChartDataGroupingForm from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { MapDataGroupingConfig } from '@admin/pages/release/datablocks/components/chart/types/mapConfig';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import ButtonText from '@common/components/ButtonText';
import Effect from '@common/components/Effect';
import Modal from '@common/components/Modal';
import {
  dataGroupingTypes,
  MapConfig,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import isEqual from 'lodash/isEqual';
import React, { ReactNode, useState } from 'react';

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
  const [editDataSetConfig, setEditDataSetConfig] = useState<{
    dataSetConfig: MapDataSetConfig;
    unit: string;
  }>();
  const { forms, updateForm, submitForms } = useChartBuilderFormsContext();

  if (!map?.dataSetConfigs.length) {
    return <p>No data groupings to edit.</p>;
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
              <tr key={key}>
                <td>{label}</td>
                <td>
                  {dataSetConfig.dataGrouping.type === 'Custom'
                    ? dataGroupingTypes[dataSetConfig.dataGrouping.type]
                    : `${
                        dataSetConfig.dataGrouping.numberOfGroups
                      } ${dataGroupingTypes[
                        dataSetConfig.dataGrouping.type
                      ].toLowerCase()}`}
                </td>
                <td className="govuk-!-text-align-right">
                  <Modal
                    open={!!editDataSetConfig}
                    triggerButton={
                      <ButtonText
                        onClick={() =>
                          setEditDataSetConfig({ dataSetConfig, unit })
                        }
                      >
                        Edit
                      </ButtonText>
                    }
                    title="Edit groupings"
                    onExit={() => setEditDataSetConfig(undefined)}
                  >
                    {editDataSetConfig && (
                      <ChartDataGroupingForm
                        dataSetConfig={editDataSetConfig.dataSetConfig}
                        dataSetConfigs={map.dataSetConfigs}
                        meta={meta}
                        unit={editDataSetConfig.unit}
                        onCancel={() => setEditDataSetConfig(undefined)}
                        onSubmit={values => {
                          onChange({
                            dataSetConfigs: map.dataSetConfigs.map(config => {
                              return isEqual(config.dataSet, values.dataSet)
                                ? values
                                : config;
                            }),
                          });

                          setEditDataSetConfig(undefined);
                        }}
                      />
                    )}
                  </Modal>
                </td>
              </tr>
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
