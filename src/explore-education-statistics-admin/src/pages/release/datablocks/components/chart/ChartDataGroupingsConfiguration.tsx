import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import ChartDataGroupingForm from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm';
import generateDataSetLabel from '@admin/pages/release/datablocks/components/chart/utils/generateDataSetLabel';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import ButtonText from '@common/components/ButtonText';
import Effect from '@common/components/Effect';
import { Form } from '@common/components/form';
import Modal from '@common/components/Modal';
import {
  AxisConfiguration,
  MapConfig,
  MapDataSetConfig,
  dataGroupingTypes,
} from '@common/modules/charts/types/chart';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { Formik } from 'formik';
import isEqual from 'lodash/isEqual';
import React, { ReactNode, useCallback, useMemo, useState } from 'react';

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
  const { updateForm, submitForms } = useChartBuilderFormsContext();

  const handleChange = useCallback(
    (values: FormValues) => onChange(values.dataSetConfigs),
    [onChange],
  );

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
    <Formik<FormValues>
      enableReinitialize
      initialValues={initialValues}
      onSubmit={async values => {
        onSubmit(values.dataSetConfigs);
        await submitForms();
      }}
    >
      {form => (
        <>
          <Form id={formId}>
            <Effect
              value={form.values}
              onChange={handleChange}
              onMount={handleChange}
            />

            <Effect
              value={{
                formKey: 'dataGroupings',
                isValid: form.isValid,
                submitCount: form.submitCount,
              }}
              onChange={updateForm}
              onMount={updateForm}
            />

            <table data-testid="chart-data-groupings">
              <thead>
                <tr>
                  <th>Data set</th>
                  <th>Groupings</th>
                  <th className="dfe-align--right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {form.values.dataSetConfigs.map(dataSetConfig => {
                  const expandedDataSet = expandDataSet(
                    dataSetConfig.dataSet,
                    meta,
                  );
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
                      <td className="dfe-align--right">
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

            <ChartBuilderSaveActions
              formId={formId}
              formKey="dataGroupings"
              disabled={form.isSubmitting}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </Form>

          {editDataSetConfig && (
            <Modal title="Edit groupings">
              <ChartDataGroupingForm
                dataSetConfig={editDataSetConfig.dataSetConfig}
                dataSetConfigs={form.values.dataSetConfigs}
                meta={meta}
                unit={editDataSetConfig.unit}
                onCancel={() => setEditDataSetConfig(undefined)}
                onSubmit={values => {
                  const updated = form.values.dataSetConfigs.map(config => {
                    if (isEqual(config.dataSet, values.dataSet)) {
                      return values;
                    }
                    return config;
                  });
                  form.setFieldValue('dataSetConfigs', updated);
                  setEditDataSetConfig(undefined);
                }}
              />
            </Modal>
          )}
        </>
      )}
    </Formik>
  );
};

export default ChartDataGroupingsConfiguration;
