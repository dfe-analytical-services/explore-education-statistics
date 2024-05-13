import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Effect from '@common/components/Effect';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldSelect from '@common/components/form/rhf/RHFFormFieldSelect';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import merge from 'lodash/merge';
import React, { ReactNode, useCallback } from 'react';

const formId = 'chartBoundaryLevelsConfigurationForm';

interface FormValues {
  boundaryLevel?: number;
}

interface Props {
  buttons?: ReactNode;
  meta: FullTableMeta;
  options: ChartOptions;
  onChange: (values: ChartOptions) => void;
  onSubmit: (chartOptions: ChartOptions) => void;
}

export default function ChartBoundaryLevelsConfiguration({
  buttons,
  meta,
  options,
  onChange,
  onSubmit,
}: Props) {
  const { updateForm, submitForms } = useChartBuilderFormsContext();

  const normalizeValues = useCallback(
    (values: FormValues): ChartOptions => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      return merge({}, options, values, {
        boundaryLevel: values.boundaryLevel
          ? parseNumber(values.boundaryLevel)
          : undefined,
      });
    },
    [options],
  );

  const handleChange = useCallback(
    (values: FormValues) => {
      onChange(normalizeValues(values));
    },
    [normalizeValues, onChange],
  );

  return (
    <FormProvider
      enableReinitialize
      initialValues={{ boundaryLevel: options.boundaryLevel }}
      validationSchema={Yup.object<FormValues>({
        boundaryLevel: Yup.number()
          .transform(value => (Number.isNaN(value) ? undefined : value))
          .nullable()
          .oneOf(meta.boundaryLevels.map(level => level.id))
          .required('Choose a boundary level'),
      })}
    >
      {({ formState, watch }) => {
        const values = watch();
        return (
          <RHFForm
            id={formId}
            onSubmit={async () => {
              onSubmit(normalizeValues(values));
              await submitForms();
            }}
          >
            <Effect value={values} onChange={handleChange} />
            <Effect
              value={{
                formKey: 'boundaryLevels',
                isValid: formState.isValid,
                submitCount: formState.submitCount,
              }}
              onChange={updateForm}
              onMount={updateForm}
            />
            <RHFFormFieldSelect<FormValues>
              label="Boundary level"
              hint="Select a version of geographical data to use"
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

            <ChartBuilderSaveActions
              formId={formId}
              formKey="boundaryLevels"
              disabled={formState.isSubmitting}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
}
