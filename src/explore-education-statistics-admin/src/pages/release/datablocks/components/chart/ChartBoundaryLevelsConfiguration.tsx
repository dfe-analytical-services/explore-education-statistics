import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Effect from '@common/components/Effect';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormProvider from '@common/components/form/FormProvider';
import { MapDataSetConfig } from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import parseNumber from '@common/utils/number/parseNumber';
import merge from 'lodash/merge';
import React, { ReactNode, useCallback } from 'react';
import ChartBoundaryLevelsDataSetConfiguration from './ChartBoundaryLevelsDataSetConfiguration';

const formId = 'chartBoundaryLevelsConfigurationForm';

export interface ChartBoundaryLevelsFormValues {
  boundaryLevel?: number;
  dataSetConfigs: MapDataSetConfig[];
}

interface Props {
  buttons?: ReactNode;
  meta: FullTableMeta;
  dataSetConfigs: MapDataSetConfig[];
  boundaryLevel: ChartOptions['boundaryLevel'];
  onChange: (values: ChartBoundaryLevelsFormValues) => void;
  onSubmit: (values: ChartBoundaryLevelsFormValues) => void;
}

export default function ChartBoundaryLevelsConfiguration({
  buttons,
  meta,
  dataSetConfigs,
  boundaryLevel,
  onChange,
  onSubmit,
}: Props) {
  const { updateForm, submitForms } = useChartBuilderFormsContext();

  const normalizeValues = useCallback(
    (values: ChartBoundaryLevelsFormValues): ChartBoundaryLevelsFormValues => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      const returnValue = merge({}, boundaryLevel, values, {
        boundaryLevel: values.boundaryLevel
          ? parseNumber(values.boundaryLevel)
          : undefined,
      });
      return returnValue;
    },
    [boundaryLevel],
  );

  const handleChange = useCallback(
    (values: ChartBoundaryLevelsFormValues) => {
      onChange(normalizeValues(values));
    },
    [normalizeValues, onChange],
  );

  return (
    <FormProvider<ChartBoundaryLevelsFormValues>
      enableReinitialize
      initialValues={{
        boundaryLevel,
        dataSetConfigs,
      }}
      /* validationSchema={Yup.object({
        boundaryLevel: Yup.number()
          .transform(value => (Number.isNaN(value) ? undefined : value))
          .nullable()
          .oneOf(meta.boundaryLevels.map(level => level.id))
          .required('Choose a boundary level'),
        // dataSetConfigs: Yup.array(), //TODO
      })} */
    >
      {({ formState, watch, control }) => {
        const values = watch();
        return (
          <Form
            id={formId}
            onSubmit={async () => {
              onSubmit(normalizeValues(values));
              await submitForms();
            }}
          >
            <Effect value={values} onChange={handleChange} />
            <Effect
              // update watcher of all forms for sibling validation
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
              hint="Select a version of geographical data to use across any data sets that don't have a specific one set for that dataset"
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
              control={control}
              meta={meta}
            />

            <ChartBuilderSaveActions
              formId={formId}
              formKey="boundaryLevels"
              disabled={formState.isSubmitting}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </Form>
        );
      }}
    </FormProvider>
  );
}
