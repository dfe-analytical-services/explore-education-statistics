import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import { ChartOptions } from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import Effect from '@common/components/Effect';
import FormGroup from '@common/components/form/FormGroup';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldCheckbox from '@common/components/form/rhf/RHFFormFieldCheckbox';
import RHFFormFieldFileInput from '@common/components/form/rhf/RHFFormFieldFileInput';
import RHFFormFieldNumberInput from '@common/components/form/rhf/RHFFormFieldNumberInput';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import RHFFormFieldSelect from '@common/components/form/rhf/RHFFormFieldSelect';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
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
import { ValidationProblemDetails } from '@common/services/types/problemDetails';
import parseNumber from '@common/utils/number/parseNumber';
import {
  mapFieldErrors,
  rhfConvertServerFieldErrors,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import capitalize from 'lodash/capitalize';
import merge from 'lodash/merge';
import pick from 'lodash/pick';
import React, {
  ChangeEvent,
  ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useRef,
} from 'react';
import { Path } from 'react-hook-form';
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
  submitError?: ValidationProblemDetails;
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
  const { updateForm, submitForms } = useChartBuilderFormsContext();

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
      titleType: Yup.mixed()
        .oneOf(['default', 'alternative'])
        .required('Choose a title type'),
      title: Yup.string().when('titleType', {
        is: 'alternative',
        then: s => s.required('Enter chart title'),
        otherwise: s => s.notRequired(),
      }),
      alt: Yup.string()
        .required('Enter chart alt text')
        .test({
          name: 'noRepeatTitle',
          message: 'Alt text should not repeat the title',
          test(value = '') {
            // eslint-disable-next-line react/no-this-in-sfc
            const title: string = this.resolve(Yup.ref('title')) ?? '';

            return value !== title;
          },
        }),
      height: Yup.number()
        .required('Enter chart height')
        .positive('Chart height must be positive'),
      width: Yup.number().positive('Chart width must be positive'),
      subtitle: Yup.string().max(
        160,
        'Subtitle must be 160 characters or less',
      ),
    });

    if (definition.capabilities.stackable) {
      schema = schema.shape({
        stacked: Yup.boolean(),
      });
    }

    if (definition.capabilities.canIncludeNonNumericData) {
      schema = schema.shape({
        includeNonNumericData: Yup.boolean(),
      });
    }

    if (definition.type === 'infographic') {
      schema = schema.shape({
        fileId: Yup.string(),
        file: Yup.file()
          .when('fileId', {
            is: (value: string) => !value,
            then: s => s.required('Select an infographic file to upload'),
          })
          .minSize(0, 'The infographic cannot be an empty file'),
      });
    }

    if (definition.capabilities.canSetBarThickness) {
      schema = schema.shape({
        barThickness: Yup.number().positive(
          'Chart bar thickness must be positive',
        ),
      });
    }

    if (definition.capabilities.canShowDataLabels) {
      schema = schema.shape({
        showDataLabels: Yup.boolean().test({
          name: 'noInlineWithDataLabels',
          message: 'Data labels cannot be used with inline legends',
          test(value) {
            return !(value && legendPosition === 'inline');
          },
        }),
      });
    }

    if (definition.capabilities.canSetDataLabelPosition) {
      schema = schema.shape({
        dataLabelPosition:
          definition.type === 'line'
            ? Yup.string()
                .oneOf<LineChartDataLabelPosition>(['above', 'below'])
                .when('showDataLabels', {
                  is: true,
                  then: s => s.required(),
                  otherwise: s => s.notRequired(),
                })
            : Yup.string()
                .oneOf<BarChartDataLabelPosition>(['inside', 'outside'])
                .when('showDataLabels', {
                  is: true,
                  then: s => s.required(),
                  otherwise: s => s.notRequired(),
                }),
      });
    }

    return schema;
  }, [
    definition.capabilities.canIncludeNonNumericData,
    definition.capabilities.canSetBarThickness,
    definition.capabilities.canSetDataLabelPosition,
    definition.capabilities.canShowDataLabels,
    definition.capabilities.stackable,
    definition.type,
    legendPosition,
  ]);

  const getInitialValues = useCallback(
    (values: ChartOptions): FormValues => {
      const getInitialDataLabelPosition = () => {
        if (
          values.dataLabelPosition &&
          dataLabelPositionOptions.find(
            position => position.value === values.dataLabelPosition,
          )
        ) {
          return values.dataLabelPosition;
        }
        return definition.type === 'line' ? 'above' : 'outside';
      };

      return {
        ...pick(values, Object.keys(validationSchema.fields)),
        dataLabelPosition: getInitialDataLabelPosition(),
      };
    },
    [dataLabelPositionOptions, definition.type, validationSchema.fields],
  );

  const chartType = useRef(definition.type);
  const initialChartOptions = useRef(getInitialValues(chartOptions));

  // Update initial options if the chart definition changes
  useEffect(() => {
    if (definition.type !== chartType.current) {
      initialChartOptions.current = getInitialValues(chartOptions);
      chartType.current = definition.type;
    }
  }, [definition.type, chartOptions, getInitialValues]);

  const normalizeValues = useCallback(
    (values: FormValues): ChartOptions => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      return merge({}, chartOptions, values, {
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
    <FormProvider
      enableReinitialize
      errorMappings={errorMappings}
      initialValues={initialChartOptions.current}
      validationSchema={validationSchema}
    >
      {({ formState, setError, watch }) => {
        const values = watch();

        if (submitError) {
          const fieldErrors = rhfConvertServerFieldErrors<FormValues>(
            submitError,
            initialChartOptions.current,
            errorMappings,
          );

          Object.keys(fieldErrors).forEach(key => {
            if (!formState.errors[key as Path<FormValues>]) {
              setError(key, {
                message: fieldErrors[key as Path<FormValues>]?.message,
              });
            }
          });
        }

        return (
          <RHFForm
            id={formId}
            onSubmit={async v => {
              onSubmit(normalizeValues(v));
              await submitForms();
            }}
          >
            <Effect
              value={{
                ...values,
              }}
              onChange={handleChange}
            />

            <Effect
              value={{
                formKey: 'options',
                isValid: formState.isValid,
                submitCount: formState.submitCount,
              }}
              onChange={updateForm}
              onMount={updateForm}
            />

            {validationSchema.fields.file && (
              <RHFFormFieldFileInput<FormValues>
                name="file"
                label="Upload new infographic"
                accept="image/*"
              />
            )}
            <div className="govuk-!-width-three-quarters">
              <RHFFormFieldRadioGroup<FormValues>
                hint="Communicate the headline message of the chart. For example 'Increase in number of people living alone'."
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
                      <RHFFormFieldTextInput<FormValues>
                        label="Enter chart title"
                        name="title"
                        hint="Use a concise descriptive title that summarises the main message in the chart."
                      />
                    ),
                  },
                ]}
              />

              <RHFFormFieldTextInput<FormValues>
                label="Subtitle"
                name="subtitle"
                hint="The statistical subtitle should say what the data is, the geography the data relates to and the time period shown.
                For example, 'Figure 1: Number of people living in one person households, England, 1991 to 2021'."
                maxLength={160}
              />
            </div>

            <RHFFormFieldTextArea<FormValues>
              className="govuk-!-width-three-quarters"
              name="alt"
              label="Alt text"
              hint="Brief and accurate description of the chart. Should not repeat the title."
              rows={3}
              onChange={replaceNewLines}
            />

            {validationSchema.fields.stacked && (
              <RHFFormFieldCheckbox<FormValues>
                name="stacked"
                label="Stacked bars"
              />
            )}

            {validationSchema.fields.height && (
              <RHFFormFieldNumberInput<FormValues>
                name="height"
                label="Height (pixels)"
                width={5}
              />
            )}

            {validationSchema.fields.width && (
              <RHFFormFieldNumberInput<FormValues>
                name="width"
                label="Width (pixels)"
                hint="Leave blank to set as full width"
                width={5}
              />
            )}

            {validationSchema.fields.barThickness && (
              <RHFFormFieldNumberInput<FormValues>
                name="barThickness"
                label="Bar thickness (pixels)"
                width={5}
              />
            )}

            {validationSchema.fields.includeNonNumericData && (
              <FormGroup>
                <RHFFormFieldCheckbox<FormValues>
                  name="includeNonNumericData"
                  label="Include data sets with non-numerical values"
                />
              </FormGroup>
            )}

            {validationSchema.fields.showDataLabels && (
              <RHFFormFieldCheckbox<FormValues>
                name="showDataLabels"
                hint={
                  legendPosition === 'inline'
                    ? 'Data labels cannot be shown when the legend is positioned inline.'
                    : undefined
                }
                label="Show data labels"
                showError={!!formState.errors.showDataLabels}
                conditional={
                  validationSchema.fields.dataLabelPosition && (
                    <RHFFormFieldSelect<FormValues>
                      label="Data label position"
                      name="dataLabelPosition"
                      order={[]}
                      options={dataLabelPositionOptions}
                    />
                  )
                }
              />
            )}

            <ChartBuilderSaveActions
              formId={formId}
              formKey="options"
              disabled={formState.isSubmitting}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
};

export default ChartConfiguration;
