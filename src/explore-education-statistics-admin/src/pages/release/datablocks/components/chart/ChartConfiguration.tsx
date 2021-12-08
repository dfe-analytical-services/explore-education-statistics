import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Effect from '@common/components/Effect';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldTextInput,
  FormGroup,
  FormSelect,
} from '@common/components/form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { ChartDefinition } from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import parseNumber from '@common/utils/number/parseNumber';
import {
  convertServerFieldErrors,
  mapFieldErrors,
  ServerValidationErrorResponse,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import merge from 'lodash/merge';
import pick from 'lodash/pick';
import React, { ChangeEvent, ReactNode, useCallback, useMemo } from 'react';
import { ObjectSchema } from 'yup';

type FormValues = Partial<ChartOptions>;

export const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'file',
    messages: {
      FILE_TYPE_INVALID: 'The infographic must be an image',
      CANNOT_OVERWRITE_FILE: 'The infographic does not have a unique filename',
      FILE_CANNOT_BE_EMPTY: 'The infographic cannot be an empty file',
      FILENAME_CANNOT_CONTAIN_SPACES_OR_SPECIAL_CHARACTERS:
        'The infographic filename cannot contain spaces or special characters',
    },
  }),
];

const replaceNewLines = (event: ChangeEvent<HTMLTextAreaElement>) => {
  // eslint-disable-next-line no-param-reassign
  event.target.value = event.target.value.replace(/\n/g, ' ');
};

interface Props {
  boundaryLevel?: number;
  buttons?: ReactNode;
  chartOptions: ChartOptions;
  definition: ChartDefinition;
  meta: FullTableMeta;
  submitError?: ServerValidationErrorResponse;
  onBoundaryLevelChange?: (boundaryLevel: string) => void;
  onChange: (chartOptions: ChartOptions) => void;
  onSubmit: (chartOptions: ChartOptions) => void;
}

const formId = 'chartConfigurationForm';

const ChartConfiguration = ({
  boundaryLevel,
  buttons,
  chartOptions,
  definition,
  meta,
  submitError,
  onBoundaryLevelChange,
  onChange,
  onSubmit,
}: Props) => {
  const { hasSubmitted, updateForm, submit } = useChartBuilderFormsContext();

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    let schema: ObjectSchema<FormValues> = Yup.object<FormValues>({
      titleType: Yup.mixed<FormValues['titleType']>()
        .oneOf(['default', 'alternative'])
        .required('Choose a title type'),
      title: Yup.string()
        .when('titleType', {
          is: 'alternative',
          then: Yup.string().required('Enter a chart title'),
          otherwise: Yup.string,
        })
        .required('Enter chart title'),
      alt: Yup.string()
        .required('Enter chart alt text')
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

    if (definition.capabilities.stackable) {
      schema = schema.shape({
        stacked: Yup.boolean(),
      });
    }

    if (definition.type === 'infographic') {
      schema = schema.shape({
        fileId: Yup.string(),
        file: Yup.file()
          .nullable()
          .when('fileId', {
            is: value => !value,
            then: Yup.file().required('Select an infographic file to upload'),
          })
          .minSize(0, 'The infographic cannot be an empty file'),
      });
    }

    if (
      definition.type === 'horizontalbar' ||
      definition.type === 'verticalbar'
    ) {
      schema = schema.shape({
        barThickness: Yup.number().positive(
          'Chart bar thickness must be positive',
        ),
      });
    }

    return schema;
  }, [definition.capabilities.stackable, definition.type]);

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
    ({ ...values }: FormValues) => {
      onChange(normalizeValues(values));
    },
    [normalizeValues, onChange],
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialErrors={
        submitError
          ? convertServerFieldErrors<FormValues>(submitError, errorMappings)
          : undefined
      }
      initialValues={initialValues}
      initialTouched={
        hasSubmitted
          ? mapValues(validationSchema.fields, () => true)
          : undefined
      }
      validateOnMount
      validationSchema={validationSchema}
      onSubmit={values => {
        onSubmit(normalizeValues(values));
        submit();
      }}
    >
      {form => {
        return (
          <Form id={formId}>
            <Effect
              value={{
                ...form.values,
              }}
              onChange={handleChange}
            />

            <Effect
              value={{
                formKey: 'options',
                isValid: form.isValid,
                submitCount: form.submitCount,
              }}
              onChange={updateForm}
              onMount={updateForm}
            />

            {validationSchema.fields.file && (
              <FormFieldFileInput<FormValues>
                name="file"
                label="Upload new infographic"
                accept="image/*"
              />
            )}

            <FormFieldRadioGroup<FormValues>
              legend="Chart title"
              legendSize="s"
              name="titleType"
              order={[]}
              options={[
                {
                  label: 'Use table title',
                  value: 'default',
                },
                {
                  label: 'Set an alternative title',
                  value: 'alternative',
                  conditional: (
                    <FormFieldTextInput<FormValues>
                      label="Enter chart title"
                      name="title"
                      hint="Use a concise descriptive title that summarises the main message in the chart."
                    />
                  ),
                },
              ]}
            />

            <FormFieldTextArea<FormValues>
              className="govuk-!-width-three-quarters"
              name="alt"
              label="Alt text"
              hint="Brief and accurate description of the chart. Should not repeat the title."
              rows={3}
              onChange={replaceNewLines}
            />

            {validationSchema.fields.stacked && (
              <FormFieldCheckbox<FormValues>
                name="stacked"
                label="Stacked bars"
              />
            )}

            {validationSchema.fields.height && (
              <FormFieldNumberInput<FormValues>
                name="height"
                label="Height (px)"
                width={5}
              />
            )}

            {validationSchema.fields.width && (
              <FormFieldNumberInput<FormValues>
                name="width"
                label="Width (px)"
                hint="Leave blank to set as full width"
                width={5}
              />
            )}

            {validationSchema.fields.barThickness && (
              <FormFieldNumberInput<FormValues>
                name="barThickness"
                label="Bar thickness (px)"
                width={5}
              />
            )}

            {definition.type === 'map' && meta.boundaryLevels && (
              <FormGroup>
                <FormSelect
                  id={`${formId}-boundaryLevel`}
                  label="Select a version of geographical data to use"
                  name="boundaryLevel"
                  order={[]}
                  value={boundaryLevel?.toString()}
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

            <ChartBuilderSaveActions formId={formId} formKey="options">
              {buttons}
            </ChartBuilderSaveActions>
          </Form>
        );
      }}
    </Formik>
  );
};

export default ChartConfiguration;
