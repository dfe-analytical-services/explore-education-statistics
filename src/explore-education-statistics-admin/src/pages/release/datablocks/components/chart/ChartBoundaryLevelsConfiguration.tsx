import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Effect from '@common/components/Effect';
import { Form, FormFieldSelect } from '@common/components/form';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
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
  onBoundaryLevelChange?: (boundaryLevel: string) => void;
  onChange: (values: ChartOptions) => void;
  onSubmit: (chartOptions: ChartOptions) => void;
}

export default function ChartBoundaryLevelsConfiguration({
  buttons,
  meta,
  options,
  onBoundaryLevelChange,
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
    <Formik<FormValues>
      enableReinitialize
      initialValues={{ boundaryLevel: options.boundaryLevel }}
      validateOnMount
      validationSchema={Yup.object<FormValues>({
        boundaryLevel: Yup.number()
          .oneOf(meta.boundaryLevels.map(level => level.id))
          .required('Choose a boundary level'),
      })}
      onSubmit={async values => {
        onSubmit(normalizeValues(values));
        await submitForms();
      }}
    >
      {form => (
        <Form id={formId}>
          <Effect value={form.values} onChange={handleChange} />
          <Effect
            value={{
              formKey: 'boundaryLevels',
              isValid: form.isValid,
              submitCount: form.submitCount,
            }}
            onChange={updateForm}
            onMount={updateForm}
          />
          <FormFieldSelect<FormValues>
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
            onChange={e => {
              onBoundaryLevelChange?.(e.target.value);
            }}
          />

          <ChartBuilderSaveActions
            formId={formId}
            formKey="boundaryLevels"
            disabled={form.isSubmitting}
          >
            {buttons}
          </ChartBuilderSaveActions>
        </Form>
      )}
    </Formik>
  );
}
