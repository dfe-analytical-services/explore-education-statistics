import { ChartBuilderForm } from '@admin/pages/release/datablocks/components/ChartBuilder';
import ChartBuilderSaveButton from '@admin/pages/release/datablocks/components/ChartBuilderSaveButton';
import {
  ChartOptions,
  FormState,
} from '@admin/pages/release/datablocks/reducers/chartBuilderReducer';
import ButtonGroup from '@common/components/ButtonGroup';
import Effect from '@common/components/Effect';
import { Form, FormFieldSelect, FormGroup } from '@common/components/form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { ChartDefinition } from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import merge from 'lodash/merge';
import pick from 'lodash/pick';
import React, { ChangeEvent, ReactNode, useCallback, useMemo } from 'react';
import { ObjectSchema, Schema } from 'yup';

const replaceNewLines = (event: ChangeEvent<HTMLTextAreaElement>) => {
  // eslint-disable-next-line no-param-reassign
  event.target.value = event.target.value.replace(/\n/g, ' ');
};

type FormValues = Partial<ChartOptions>;

interface Props {
  buttons?: ReactNode;
  canSaveChart: boolean;
  chartOptions: ChartOptions;
  definition: ChartDefinition;
  forms: Dictionary<ChartBuilderForm>;
  hasSubmittedChart: boolean;
  meta: FullTableMeta;
  onBoundaryLevelChange?: (boundaryLevel: string) => void;
  onChange: (chartOptions: ChartOptions) => void;
  onFormStateChange: (
    state: {
      form: 'options';
    } & FormState,
  ) => void;
  onSubmit: (chartOptions: ChartOptions) => void;
}

const formId = 'chartConfigurationForm';

const ChartConfiguration = ({
  buttons,
  canSaveChart,
  chartOptions,
  definition,
  forms,
  hasSubmittedChart,
  meta,
  onBoundaryLevelChange,
  onChange,
  onFormStateChange,
  onSubmit,
}: Props) => {
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    let schema: ObjectSchema<FormValues> = Yup.object<FormValues>({
      title: Yup.string().required('Enter chart title'),
      alt: Yup.string()
        .required('Enter chart alt text')
        .max(125, 'Alt text must be less than 125 characters')
        .test({
          name: 'noRepeatTitle',
          message: 'Alt text should not repeat the title',
          test(value = '') {
            // eslint-disable-next-line react/no-this-in-sfc
            const title: string = this.resolve(Yup.ref('title')) ?? '';

            return value.trim() !== title.trim();
          },
        }),
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

    if (definition.type === 'infographic') {
      schema = schema.shape({
        fileId: Yup.string(),
        file: Yup.mixed()
          .when('fileId', {
            is: value => !value,
            then: Yup.mixed().required('Select an infographic file to upload'),
          })
          .test({
            name: 'imageType',
            message: 'The selected infographic must be an image',
            test(value?: File) {
              if (!value) {
                return true;
              }

              return value?.type.startsWith('image');
            },
          }),
      });
    }

    return schema;
  }, [
    definition.capabilities.hasLegend,
    definition.capabilities.stackable,
    definition.type,
  ]);

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
    <Formik<FormValues>
      enableReinitialize
      initialValues={initialValues}
      initialTouched={
        hasSubmittedChart
          ? mapValues(validationSchema.fields, () => true)
          : undefined
      }
      validateOnMount
      validationSchema={validationSchema}
      onSubmit={values => {
        if (canSaveChart) {
          onSubmit(normalizeValues(values));
        }
      }}
    >
      {form => {
        return (
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
                submitCount: form.submitCount,
              }}
              onChange={onFormStateChange}
              onMount={onFormStateChange}
            />

            {validationSchema.fields.file && (
              <FormFieldFileInput<FormValues>
                id={`${formId}-file`}
                name="file"
                label="Upload new infographic"
                accept="image/*"
              />
            )}

            <FormFieldTextArea<FormValues>
              id={`${formId}-title`}
              name="title"
              label="Title"
              className="govuk-!-width-three-quarters"
              rows={3}
              hint="Use a concise descriptive title that summarises the main message in the chart."
              onChange={replaceNewLines}
            />

            <FormFieldTextArea<FormValues>
              id={`${formId}-alt`}
              className="govuk-!-width-three-quarters"
              name="alt"
              label="Alt text"
              hint="Brief and accurate description of the chart in less than 125 characters. Should not repeat the title."
              maxLength={125}
              rows={2}
              onChange={replaceNewLines}
            />

            {validationSchema.fields.stacked && (
              <FormFieldCheckbox<FormValues>
                id={`${formId}-stacked`}
                name="stacked"
                label="Stacked bars"
              />
            )}

            {validationSchema.fields.legend && (
              <FormFieldSelect<FormValues>
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
              <FormFieldNumberInput<FormValues>
                id={`${formId}-height`}
                name="height"
                label="Height (px)"
                width={5}
              />
            )}

            {validationSchema.fields.width && (
              <FormFieldNumberInput<FormValues>
                id={`${formId}-width`}
                name="width"
                label="Width (px)"
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
                    <FormFieldSelect<FormValues>
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

            <ButtonGroup>
              <ChartBuilderSaveButton
                formId={formId}
                forms={forms}
                showSubmitError={
                  form.isValid && form.submitCount > 0 && !canSaveChart
                }
              />

              {buttons}
            </ButtonGroup>
          </Form>
        );
      }}
    </Formik>
  );
};

export default ChartConfiguration;
