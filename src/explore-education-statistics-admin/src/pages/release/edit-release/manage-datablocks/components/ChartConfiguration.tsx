import { ChartOptions } from '@admin/pages/release/edit-release/manage-datablocks/reducers/chartBuilderReducer';
import Button from '@common/components/Button';
import Effect from '@common/components/Effect';
import ErrorSummary from '@common/components/ErrorSummary';
import {
  Form,
  FormFieldSelect,
  FormFieldTextInput,
  FormGroup,
  Formik,
} from '@common/components/form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import { ChartDefinition } from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import merge from 'lodash/merge';
import pick from 'lodash/pick';
import React, { useCallback, useMemo } from 'react';
import { ObjectSchema, Schema } from 'yup';
import InfographicChartForm from './InfographicChartForm';

type FormValues = Partial<ChartOptions>;

interface Props {
  canSaveChart: boolean;
  definition: ChartDefinition;
  chartOptions: ChartOptions;
  releaseId: string;
  meta: FullTableMeta;
  onBoundaryLevelChange?: (boundaryLevel: string) => void;
  onChange: (chartOptions: ChartOptions) => void;
  onFormStateChange: (state: { form: 'options'; isValid: boolean }) => void;
  onSubmit: (chartOptions: ChartOptions) => void;
}

const formId = 'chartConfigurationForm';

const ChartConfiguration = ({
  canSaveChart,
  chartOptions,
  definition,
  meta,
  releaseId,
  onBoundaryLevelChange,
  onChange,
  onFormStateChange,
  onSubmit,
}: Props) => {
  const { fileId } = chartOptions;

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    let schema: ObjectSchema<FormValues> = Yup.object<FormValues>({
      title: Yup.string(),
      height: Yup.number()
        .required('Enter chart height')
        .positive('Chart height must be positive'),
      width: Yup.number().positive('Chart width must be positive'),
    });

    if (definition.capabilities.hasLegend) {
      schema = schema.shape({
        legend: Yup.string().oneOf(
          ['bottom', 'top', 'none'],
          'Select a valid legend position',
        ) as Schema<ChartOptions['legend']>,
      });
    }

    if (definition.capabilities.stackable) {
      schema = schema.shape({
        stacked: Yup.boolean(),
      });
    }

    return schema;
  }, [definition.capabilities.hasLegend, definition.capabilities.stackable]);

  const initialValues = useMemo<FormValues>(() => {
    return pick(chartOptions, Object.keys(validationSchema.fields));
  }, [chartOptions, validationSchema]);

  const normalizeValues = useCallback(
    (values: FormValues): ChartOptions => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      return merge({}, chartOptions, values, {
        width: parseNumber(values.width),
      });
    },
    [chartOptions],
  );

  const handleChange = useCallback(
    ({ isValid, ...values }: FormValues & { isValid: boolean }) => {
      if (isValid) {
        onChange(normalizeValues(values));
      }
    },
    [normalizeValues, onChange],
  );

  return (
    <>
      {definition.type === 'infographic' && (
        <>
          <InfographicChartForm
            canSaveChart={canSaveChart}
            releaseId={releaseId}
            fileId={fileId}
            onSubmit={async nextFileId => {
              onChange({
                ...chartOptions,
                fileId: nextFileId,
              });
            }}
            onDelete={async () => {
              onSubmit({
                ...chartOptions,
                fileId: '',
              });
            }}
          />
          <hr />
        </>
      )}

      <Formik<FormValues>
        initialValues={initialValues}
        enableReinitialize
        onSubmit={values => {
          onSubmit(normalizeValues(values));
        }}
        isInitialValid={validationSchema.isValidSync(initialValues)}
        validationSchema={validationSchema}
        render={form => (
          <Form id={formId}>
            <Effect
              value={{
                ...form.values,
                isValid: form.isValid,
              }}
              onChange={handleChange}
            />

            <Effect
              value={{
                form: 'options',
                isValid: form.isValid,
              }}
              onChange={onFormStateChange}
              onMount={onFormStateChange}
            />

            <FormFieldTextInput<ChartOptions>
              id={`${formId}-title`}
              name="title"
              label="Chart title"
              percentageWidth="three-quarters"
            />

            {validationSchema.fields.stacked && (
              <FormFieldCheckbox<ChartOptions>
                id={`${formId}-stacked`}
                name="stacked"
                label="Stacked bars"
              />
            )}

            {validationSchema.fields.legend && (
              <FormFieldSelect<ChartOptions>
                id={`${formId}-position`}
                name="legend"
                label="Legend position"
                options={[
                  { label: 'Top', value: 'top' },
                  { label: 'Bottom', value: 'bottom' },
                  { label: 'None', value: 'none' },
                ]}
                order={[]}
              />
            )}

            {validationSchema.fields.height && (
              <FormFieldNumberInput<ChartOptions>
                id={`${formId}-height`}
                name="height"
                label="Chart height (px)"
                width={5}
              />
            )}

            {validationSchema.fields.width && (
              <FormFieldNumberInput<ChartOptions>
                id={`${formId}-width`}
                name="width"
                label="Chart width (px)"
                hint="Leave blank to set as full width"
                width={5}
              />
            )}

            {definition.type === 'map' && meta.boundaryLevels && (
              <>
                {meta.boundaryLevels.length === 1 && (
                  <div>
                    Using <em>{meta.boundaryLevels[0].label}</em>
                  </div>
                )}
                {meta.boundaryLevels.length > 1 && (
                  <FormGroup>
                    <FormFieldSelect<ChartOptions>
                      id={`${formId}-geographicId`}
                      label="Select a version of geographical data to use"
                      name="geographicId"
                      order={[]}
                      options={[
                        { label: 'Latest', value: '' },
                        ...meta.boundaryLevels.map(({ id, label }) => ({
                          value: id,
                          label,
                        })),
                      ]}
                      onChange={e => {
                        if (onBoundaryLevelChange) {
                          onBoundaryLevelChange(e.target.value);
                        }
                      }}
                    />
                  </FormGroup>
                )}
              </>
            )}

            {form.isValid && form.submitCount > 0 && !canSaveChart && (
              <ErrorSummary
                title="Cannot save chart"
                id={`${formId}-errorSummary`}
                errors={[
                  {
                    id: `${formId}-submit`,
                    message: 'Ensure that all other tabs are valid first',
                  },
                ]}
              />
            )}

            <Button type="submit" id={`${formId}-submit`}>
              Save chart options
            </Button>
          </Form>
        )}
      />
    </>
  );
};

export default ChartConfiguration;
