import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import ChartDataGroupingForm from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import ButtonText from '@common/components/ButtonText';
import Effect from '@common/components/Effect';
import Modal from '@common/components/Modal';
import {
  AxisConfiguration,
  MapConfig,
  MapDataSetConfig,
  dataGroupingTypes,
} from '@common/modules/charts/types/chart';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import isEqual from 'lodash/isEqual';
import React, { ReactNode, useMemo, useState } from 'react';
import { ChartOptions } from './reducers/chartBuilderReducer';

const formId = 'chartDataGroupingsConfigurationForm';

interface FormValues {
  dataSetConfigs: MapDataSetConfig[];
}

interface Props {
  axisMajor: AxisConfiguration;
  buttons?: ReactNode;
  data: TableDataResult[];
  legend: LegendConfiguration;
  map?: MapConfig;
  meta: FullTableMeta;
  options: ChartOptions;
  onChange: (dataSetConfigs: MapDataSetConfig[]) => void;
  onSubmit: (dataSetConfigs: MapDataSetConfig[]) => void;
}

const ChartDataGroupingsConfiguration = ({
  axisMajor,
  buttons,
  data,
  legend,
  map,
  meta,
  options,
  onChange,
  onSubmit,
}: Props) => {
  const [editDataSetConfig, setEditDataSetConfig] = useState<{
    dataSetConfig: MapDataSetConfig;
    unit: string;
  }>();
  const { forms, updateForm, submitForms } = useChartBuilderFormsContext();

  const initialValues = useMemo<FormValues>(() => {
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
      dataSetConfigs: dataSetCategoryConfigs.map(
        ({ rawDataSet, dataGrouping }) => ({
          dataSet: rawDataSet,
          dataGrouping:
            map?.dataSetConfigs.find(config =>
              isEqual(config.dataSet, rawDataSet),
            )?.dataGrouping ?? dataGrouping,
        }),
      ),
    };
  }, [axisMajor, data, meta, legend.items, map, options]);

  if (!initialValues.dataSetConfigs?.length) {
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
          {initialValues.dataSetConfigs.map(dataSetConfig => {
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
                  <ButtonText
                    onClick={() =>
                      setEditDataSetConfig({ dataSetConfig, unit })
                    }
                  >
                    Edit
                  </ButtonText>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>

      {editDataSetConfig && (
        <Modal
          open
          title="Edit groupings"
          onExit={() => setEditDataSetConfig(undefined)}
        >
          <ChartDataGroupingForm
            dataSetConfig={editDataSetConfig.dataSetConfig}
            dataSetConfigs={initialValues.dataSetConfigs}
            meta={meta}
            unit={editDataSetConfig.unit}
            onCancel={() => setEditDataSetConfig(undefined)}
            onSubmit={values => {
              const updated = initialValues.dataSetConfigs.map(config => {
                if (isEqual(config.dataSet, values.dataSet)) {
                  return values;
                }
                return config;
              });
              onChange(updated);
              setEditDataSetConfig(undefined);
            }}
          />
        </Modal>
      )}

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
          onSubmit(initialValues.dataSetConfigs);
          await submitForms();
        }}
      >
        {buttons}
      </ChartBuilderSaveActions>
    </>
  );
};

export default ChartDataGroupingsConfiguration;
