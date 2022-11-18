import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Effect from '@common/components/Effect';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldTextInput,
  FormFieldSelect,
} from '@common/components/form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldFileInput from '@common/components/form/FormFieldFileInput';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormGroup from '@common/components/form/FormGroup';
import {
  ChartDefinition,
  BarChartDataLabelPosition,
  LineChartDataLabelPosition,
} from '@common/modules/charts/types/chart';
import { LegendPosition } from '@common/modules/charts/types/legend';
import {
  barChartDataLabelPositions,
  lineChartDataLabelPositions,
} from '@common/modules/charts/util/chartUtils';
import parseNumber from '@common/utils/number/parseNumber';
import {
  convertServerFieldErrors,
  mapFieldErrors,
  ServerValidationErrorResponse,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import capitalize from 'lodash/capitalize';
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
      FileTypeInvalid: 'The infographic must be an image',
      CannotOverwriteFile: 'The infographic does not have a unique filename',
      FileCannotBeEmpty: 'The infographic cannot be an empty file',
      FilenameCannotContainSpacesOrSpecialCharacters:
        'The infographic filename cannot contain spaces or special characters',
    },
  }),
];

const replaceNewLines = (event: ChangeEvent<HTMLTextAreaElement>) => {
  // eslint-disable-next-line no-param-reassign
  event.target.value = event.target.value.replace(/\n/g, ' ');
};

interface Props {
  buttons?: ReactNode;
  chartOptions: ChartOptions;
  definition: ChartDefinition;
  legendPosition?: LegendPosition;
  submitError?: ServerValidationErrorResponse;
  onChange: (chartOptions: ChartOptions) => void;
  onSubmit: (chartOptions: ChartOptions) => void;
}

const formId = 'chartConfigurationForm';

const ChartConfiguration = ({
  buttons,
  chartOptions,
  definition,
  legendPosition,
  submitError,
  onChange,
  onSubmit,
}: Props) => {
  const {
    hasSubmitted,
    updateForm,
    submitForms,
  } = useChartBuilderFormsContext();

  const dataLabelPositionOptions = useMemo(() => {
    const options =
      definition.type === 'line'
        ? lineChartDataLabelPositions
        : barChartDataLabelPositions;

    return options.map(position => {
      return { label: capitalize(position), value: position };
    });
  }, [definition.type]);

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
      includeNonNumericData: Yup.boolean(),
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

    if (
      definition.type === 'horizontalbar' ||
      definition.type === 'verticalbar' ||
      definition.type === 'line'
    ) {
      schema = schema.shape({
        showDataLabels: Yup.boolean().test({
          name: 'noInlineWithDataLabels',
          message: 'Data labels cannot be used with inline legends',
          test(value) {
            return !(value && legendPosition === 'inline');
          },
        }),
        dataLabelPosition:
          definition.type === 'line'
            ? Yup.string().oneOf<LineChartDataLabelPosition>(['above', 'below'])
            : Yup.string().oneOf<BarChartDataLabelPosition>([
                'inside',
                'outside',
              ]),
      });
    }

    return schema;
  }, [definition.capabilities.stackable, definition.type, legendPosition]);

  const initialValues = useMemo<FormValues>(() => {
    const getInitialDataLabelPosition = () => {
      if (
        chartOptions.dataLabelPosition &&
        dataLabelPositionOptions.find(
          position => position.value === chartOptions.dataLabelPosition,
        )
      ) {
        return chartOptions.dataLabelPosition;
      }
      return definition.type === 'line' ? 'above' : 'outside';
    };

    return {
      ...pick(chartOptions, Object.keys(validationSchema.fields)),
      dataLabelPosition: getInitialDataLabelPosition(),
    };
  }, [
    chartOptions,
    dataLabelPositionOptions,
    definition.type,
    validationSchema,
  ]);

  const normalizeValues = useCallback(
    (values: FormValues): ChartOptions => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      return merge({}, chartOptions, values, {
        width: parseNumber(values.width),
        boundaryLevel: values.boundaryLevel
          ? parseNumber(values.boundaryLevel)
          : undefined,
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
      onSubmit={async values => {
        onSubmit(normalizeValues(values));
        await submitForms();
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
                label="Height (pixels)"
                width={5}
              />
            )}

            {validationSchema.fields.width && (
              <FormFieldNumberInput<FormValues>
                name="width"
                label="Width (pixels)"
                hint="Leave blank to set as full width"
                width={5}
              />
            )}

            {validationSchema.fields.barThickness && (
              <FormFieldNumberInput<FormValues>
                name="barThickness"
                label="Bar thickness (pixels)"
                width={5}
              />
            )}

            {definition.type !== 'map' && (
              <FormGroup>
                <FormFieldCheckbox
                  name="includeNonNumericData"
                  label="Include data sets with non-numerical values"
                />
              </FormGroup>
            )}

            {validationSchema.fields.showDataLabels && (
              <FormFieldCheckbox<FormValues>
                name="showDataLabels"
                hint={
                  legendPosition === 'inline'
                    ? 'Data labels cannot be shown when the legend is positioned inline.'
                    : undefined
                }
                label="Show data labels"
                showError={!!form.errors.showDataLabels}
                conditional={
                  <FormFieldSelect<FormValues>
                    label="Data label position"
                    name="dataLabelPosition"
                    order={[]}
                    options={dataLabelPositionOptions}
                  />
                }
              />
            )}

            <ChartBuilderSaveActions
              formId={formId}
              formKey="options"
              disabled={form.isSubmitting}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </Form>
        );
      }}
    </Formik>
  );
};

export default ChartConfiguration;
