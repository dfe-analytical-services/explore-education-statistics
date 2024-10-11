import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Effect from '@common/components/Effect';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormProvider from '@common/components/form/FormProvider';
import {
  AxisConfiguration,
  MapConfig,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import getDataSetCategoryConfigs from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import { isEqual } from 'lodash';
import merge from 'lodash/merge';
import React, { ReactNode, useCallback, useMemo } from 'react';
import { AnyObject, NumberSchema, ObjectSchema } from 'yup';
import ChartBoundaryLevelsDataSetConfiguration from './ChartBoundaryLevelsDataSetConfiguration';

const formId = 'chartBoundaryLevelsConfigurationForm';

export interface ChartBoundaryLevelsFormValues {
  boundaryLevel?: number;
  dataSetConfigs: Omit<MapDataSetConfig, 'dataGrouping'>[];
}

interface Props {
  buttons?: ReactNode;
  axisMajor: AxisConfiguration;
  data: TableDataResult[];
  legend: LegendConfiguration;
  map?: MapConfig;
  meta: FullTableMeta;
  options: ChartOptions;
  onChange: (values: ChartBoundaryLevelsFormValues) => void;
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
  const { updateForm, submitForms } = useChartBuilderFormsContext();

  const normalizeValues = useCallback(
    (values: ChartBoundaryLevelsFormValues): ChartBoundaryLevelsFormValues => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      return {
        boundaryLevel: values.boundaryLevel
          ? parseNumber(values.boundaryLevel)
          : undefined,
        dataSetConfigs: values.dataSetConfigs.map(
          ({ boundaryLevel, dataSet }) => ({
            boundaryLevel: parseNumber(boundaryLevel),
            dataSet,
          }),
        ),
      };
    },
    [],
  );

  const handleChange = useCallback(
    (values: ChartBoundaryLevelsFormValues) => {
      onChange(normalizeValues(values));
    },
    [normalizeValues, onChange],
  );

  const validationSchema = useMemo<
    ObjectSchema<ChartBoundaryLevelsFormValues>
  >(() => {
    return Yup.object({
      boundaryLevel: Yup.number()
        .transform(value => (Number.isNaN(value) ? undefined : value))
        .nullable()
        .oneOf(meta.boundaryLevels.map(level => level.id))
        .required('Choose a boundary level'),
      dataSetConfigs: Yup.array()
        .of(
          Yup.object({
            boundaryLevel: Yup.mixed<number | ''>().test(
              'dataset-boundary-is-number-or-empty',
              'Must be a number or an empty string',
              value => !Number.isNaN(value) || value === '',
            ) as NumberSchema<number, AnyObject, undefined, ''>,
            dataSet: Yup.object({
              filters: Yup.array().required(),
            }).required(),
          }),
        )
        .required(),
    });
  }, [meta.boundaryLevels]);

  const initialValues = useMemo<ChartBoundaryLevelsFormValues>(() => {
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

  return (
    <FormProvider<ChartBoundaryLevelsFormValues>
      enableReinitialize
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      {({ formState, watch }) => {
        const values = watch();
        return (
          <Form id={formId} onSubmit={onSubmit}>
            <Effect value={values} onChange={handleChange} />
            <Effect
              value={{
                formKey: 'boundaryLevels',
                isValid: formState.isValid,
                submitCount: 0,
              }}
              onChange={updateForm}
              onMount={updateForm}
            />
            <FormFieldSelect<ChartBoundaryLevelsFormValues>
              label="Default Boundary level"
              hint="Select a version of geographical data to use across any data sets that don't have a specific one set"
              name="boundaryLevel"
              order={[]}
              options={[
                {
                  label: 'Please select',
                  value: '',
                },
                ...meta.boundaryLevels.map(({ id, label }) => ({
                  value: id,
                  label,
                })),
              ]}
            />

            <ChartBoundaryLevelsDataSetConfiguration
              dataSetConfigs={values.dataSetConfigs}
              meta={meta}
            />

            <ChartBuilderSaveActions
              formId={formId}
              formKey="boundaryLevels"
              disabled={formState.isSubmitting}
              onClick={async () => {
                onSubmit(values);
                await submitForms();
              }}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </Form>
        );
      }}
    </FormProvider>
  );
}
