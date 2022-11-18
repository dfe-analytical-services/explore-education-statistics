import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Effect from '@common/components/Effect';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldSelect,
} from '@common/components/form';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import {
  CustomDataGroup,
  DataClassification,
} from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import merge from 'lodash/merge';
import omit from 'lodash/omit';
import pick from 'lodash/pick';
import React, { ReactNode, useCallback, useMemo } from 'react';
import { StringSchema } from 'yup';

const formId = 'chartMapConfigurationForm';

interface FormValues {
  boundaryLevel?: number;
  customDataGroups: CustomDataGroup[];
  dataClassification: DataClassification;
  dataGroups: number;
}

interface Props {
  buttons?: ReactNode;
  dataSetsUnits?: string[];
  meta: FullTableMeta;
  options: ChartOptions;
  onBoundaryLevelChange?: (boundaryLevel: string) => void;
  onChange: (values: ChartOptions) => void;
  onSubmit: (chartOptions: ChartOptions) => void;
}

export default function ChartMapConfiguration({
  buttons,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  dataSetsUnits = [], // EES-3858
  meta,
  options,
  onBoundaryLevelChange,
  onChange,
  onSubmit,
}: Props) {
  const {
    hasSubmitted,
    updateForm,
    submitForms,
  } = useChartBuilderFormsContext();

  // EES-3858 - removed until feature is reimplemented
  // const hasMixedUnits = !dataSetsUnits.every(unit => unit === dataSetsUnits[0]);

  const validationSchema = useMemo(() => {
    let schema = Yup.object<FormValues>({
      customDataGroups: Yup.array()
        .of(
          Yup.object({
            max: Yup.number(),
            min: Yup.number(),
          }),
        )
        .max(100, 'The number of data groups cannot be greater than 100')
        .when('dataClassification', {
          is: 'Custom',
          then: Yup.array().required('There must be at least 1 data group'),
        }),
      dataClassification: Yup.string()
        .required('Choose a data classification')
        .oneOf(['EqualIntervals', 'Quantiles', 'Custom']) as StringSchema<
        DataClassification
      >,
      dataGroups: Yup.number()
        .required('Enter the number of data groups')
        .min(1, 'The number of data groups must be greater than 1')
        .max(100, 'The number of data groups cannot be greater than 100'),
    });

    if (meta.boundaryLevels?.length) {
      schema = schema.shape({
        boundaryLevel: Yup.number()
          .oneOf(meta.boundaryLevels.map(level => level.id))
          .required('Choose a boundary level'),
      });
    }

    return schema;
  }, [meta.boundaryLevels]);

  const initialValues = useMemo<FormValues>(() => {
    return pick(options, Object.keys(validationSchema.fields)) as FormValues;
  }, [options, validationSchema]);

  const normalizeValues = useCallback(
    (values: FormValues): ChartOptions => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      const result = merge({}, options, values, {
        boundaryLevel: values.boundaryLevel
          ? parseNumber(values.boundaryLevel)
          : undefined,
      });
      // customDataGroups are removable, so don't merge - update instead
      result.customDataGroups = [...(values.customDataGroups ?? [])];
      return result;
    },
    [options],
  );

  const handleChange = useCallback(
    ({ ...values }: FormValues) => {
      onChange(normalizeValues(values));
    },
    [normalizeValues, onChange],
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={initialValues}
      initialTouched={
        hasSubmitted
          ? {
              ...mapValues(
                omit(validationSchema.fields, 'customDataGroups'),
                () => true,
              ),
              customDataGroups: [],
            }
          : undefined
      }
      validateOnMount
      validationSchema={validationSchema}
      onSubmit={async values => {
        onSubmit(normalizeValues(values));
        await submitForms();
      }}
    >
      {form => (
        <Form id={formId}>
          <Effect
            value={{
              ...form.values,
            }}
            onChange={handleChange}
          />

          <Effect
            value={{
              formKey: 'map',
              isValid: form.isValid,
              submitCount: form.submitCount,
            }}
            onChange={updateForm}
            onMount={updateForm}
          />

          {meta.boundaryLevels?.length && (
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
          )}

          <FormFieldRadioGroup<FormValues>
            legend="Data classification"
            legendSize="s"
            name="dataClassification"
            options={[
              {
                label: 'Equal intervals',
                value: 'EqualIntervals',
                hint: 'Data is classified into groups with equal-sized ranges.',
              },
              {
                label: 'Quantiles',
                value: 'Quantiles',
                hint:
                  'Data is classified so that each group roughly has the same quantity of features.',
              },
              // EES-3858 - removed until feature is reimplemented
              // {
              //   label: 'Custom',
              //   value: 'Custom',
              //   hint: 'Define custom groups.',
              // },
            ]}
            order={[]}
          />

          {form.values.dataClassification !== 'Custom' && (
            <FormFieldNumberInput<FormValues>
              name="dataGroups"
              label="Number of data groups"
              hint="The number of groups that the data will be classified into."
              width={3}
            />
          )}

          {/* EES-3858 - removed until feature is reimplemented
           {form.values.dataClassification === 'Custom' && (
            <ChartMapCustomGroupsConfiguration
              groups={form.values.customDataGroups ?? []}
              id={`${formId}-customDataGroups`}
              showError={
                hasSubmitted &&
                !isValid &&
                form.values.dataClassification === 'Custom' &&
                !form.values.customDataGroups.length
              }
              unit={!hasMixedUnits ? dataSetsUnits[0] : undefined}
              onAddGroup={group =>
                form.setFieldValue(
                  'customDataGroups',
                  orderBy([...form.values.customDataGroups, group], g => g.min),
                )
              }
              onRemoveGroup={group =>
                form.setFieldValue(
                  'customDataGroups',
                  form.values.customDataGroups?.filter(
                    g => !(g.min === group.min && g.max === group.max),
                  ),
                )
              }
            />
          )} */}

          <ChartBuilderSaveActions
            formId={formId}
            formKey="map"
            disabled={form.isSubmitting}
          >
            {buttons}
          </ChartBuilderSaveActions>
        </Form>
      )}
    </Formik>
  );
}
