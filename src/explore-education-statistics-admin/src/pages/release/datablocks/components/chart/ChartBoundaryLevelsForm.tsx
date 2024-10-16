import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import Effect from '@common/components/Effect';
import Form from '@common/components/form/Form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormProvider from '@common/components/form/FormProvider';
import { MapDataSetConfig } from '@common/modules/charts/types/chart';
import Yup from '@common/validation/yup';
import React, { ReactNode, useMemo } from 'react';
import { AnyObject, NumberSchema, ObjectSchema } from 'yup';

const formId = 'chartBoundaryLevelsConfigurationForm';

export interface ChartBoundaryLevelsFormValues {
  boundaryLevel?: number;
  dataSetConfigs: Omit<MapDataSetConfig, 'dataGrouping'>[];
}

interface Props {
  hasDataSetBoundaryLevels?: boolean; // feature flag (remove once)
  buttons?: ReactNode;
  boundaryLevelOptions: { label: string; value: number }[];
  initialValues?: ChartBoundaryLevelsFormValues;
  dataSetRows: { key: string; label: string }[];
  onChange: (values: Partial<ChartBoundaryLevelsFormValues>) => void;
  onSubmit: (values: ChartBoundaryLevelsFormValues) => void;
}

export default function ChartBoundaryLevelsForm({
  hasDataSetBoundaryLevels = true,
  buttons,
  boundaryLevelOptions,
  dataSetRows,
  initialValues,
  onChange,
  onSubmit,
}: Props) {
  const { updateForm, submitForms } = useChartBuilderFormsContext();

  const validationSchema = useMemo<
    ObjectSchema<ChartBoundaryLevelsFormValues>
  >(() => {
    return Yup.object({
      boundaryLevel: Yup.number()
        .transform(value => (Number.isNaN(value) ? undefined : value))
        .nullable()
        .oneOf(boundaryLevelOptions.map(({ value }) => value))
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
  }, [boundaryLevelOptions]);

  return (
    <FormProvider
      validationSchema={validationSchema}
      initialValues={initialValues}
    >
      {({ formState, watch }) => {
        const values = watch();
        return (
          <Form<ChartBoundaryLevelsFormValues>
            id={formId}
            onSubmit={onSubmit}
            onChange={onChange}
          >
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
              label={
                hasDataSetBoundaryLevels
                  ? 'Default boundary level'
                  : 'Boundary level'
              }
              hint={`Select a version of geographical data to use${
                hasDataSetBoundaryLevels
                  ? "across any data sets that don't have a specific one set"
                  : ''
              }`}
              name="boundaryLevel"
              order={[]}
              options={[
                {
                  label: 'Please select',
                  value: '',
                },
                ...boundaryLevelOptions,
              ]}
            />
            {hasDataSetBoundaryLevels && dataSetRows.length > 1 && (
              <>
                <h4>Set boundary levels per data set</h4>
                <table data-testid="chart-dataset-boundary-levels">
                  <thead>
                    <tr>
                      <th>Data set</th>
                      <th>Boundary</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dataSetRows.map(({ key, label }, index) => {
                      return (
                        <tr key={key}>
                          <td>{label}</td>
                          <td>
                            <FormFieldSelect
                              label={`Boundary level for dataset: ${label}`}
                              hideLabel
                              name={`dataSetConfigs[${index}].boundaryLevel`}
                              order={[]}
                              options={[
                                {
                                  label: 'Use default',
                                  value: '',
                                },
                                ...boundaryLevelOptions,
                              ]}
                            />
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </>
            )}

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
