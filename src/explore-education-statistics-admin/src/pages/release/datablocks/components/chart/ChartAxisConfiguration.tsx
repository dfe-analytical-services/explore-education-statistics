import styles from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration.module.scss';
import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import ChartReferenceLinesConfiguration from '@admin/pages/release/datablocks/components/chart/ChartReferenceLinesConfiguration';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import Effect from '@common/components/Effect';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldSelect,
  FormFieldset,
  FormFieldTextInput,
} from '@common/components/form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import { SelectOption } from '@common/components/form/FormSelect';
import {
  AxesConfiguration,
  AxisConfiguration,
  AxisGroupBy,
  AxisType,
  ChartDefinition,
  Label,
  TickConfig,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import createDataSetCategories, {
  toChartData,
} from '@common/modules/charts/util/createDataSetCategories';
import { calculateMinorAxisDomainValues } from '@common/modules/charts/util/domainTicks';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { OmitStrict } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import merge from 'lodash/merge';
import pick from 'lodash/pick';
import React, { ReactNode, useCallback, useMemo } from 'react';
import { ObjectSchema } from 'yup';

type FormValues = Partial<OmitStrict<AxisConfiguration, 'dataSets' | 'type'>>;

interface Props {
  axesConfiguration: AxesConfiguration;
  buttons?: ReactNode;
  id: string;
  type: AxisType;
  definition: ChartDefinition;
  data: TableDataResult[];
  meta: FullTableMeta;
  includeNonNumericData?: boolean;
  onChange: (configuration: AxisConfiguration) => void;
  onSubmit: (configuration: AxisConfiguration) => void;
}

const ChartAxisConfiguration = ({
  axesConfiguration,
  buttons,
  definition,
  id,
  data,
  meta,
  includeNonNumericData,
  type,
  onChange,
  onSubmit,
}: Props) => {
  const { capabilities } = definition;

  const {
    hasSubmitted,
    updateForm,
    submitForms,
  } = useChartBuilderFormsContext();

  const axisDefinition = definition.axes[type];
  const axisConfiguration = axesConfiguration[type];

  const dataSetCategories = useMemo<DataSetCategory[]>(() => {
    if (!axesConfiguration.major) {
      return [];
    }

    const config: AxisConfiguration = {
      ...axesConfiguration.major,
      min: 0,
      max: undefined,
    };

    return createDataSetCategories(config, data, meta, includeNonNumericData);
  }, [axesConfiguration.major, data, meta, includeNonNumericData]);

  const chartData = dataSetCategories.map(toChartData);
  const minorAxisDomain = axesConfiguration.minor
    ? calculateMinorAxisDomainValues(chartData, axesConfiguration.minor)
    : undefined;

  const groupByOptions = useMemo<RadioOption<AxisGroupBy>[]>(() => {
    const options: RadioOption<AxisGroupBy>[] = [
      {
        label: 'Time periods',
        value: 'timePeriod',
      },
      {
        label: 'Locations',
        value: 'locations',
      },
      {
        label: 'Indicators',
        value: 'indicators',
      },
    ];

    // Don't show 'Filters' option if we would end up with no
    // categories due to every filter being siblingless filters.
    const canGroupByFilters = Object.values(meta.filters).some(
      filterGroup => filterGroup.options.length > 1,
    );

    if (!canGroupByFilters) {
      return options;
    }

    const categories: SelectOption[] = Object.entries(meta.filters)
      .filter(([, value]) => value.options.length > 1)
      .map(([key, value]) => {
        return {
          label: key,
          value: value.name,
        };
      });

    categories.unshift({ label: 'All filters', value: '' });

    options.push({
      label: 'Filters',
      value: 'filters',
      conditional: (
        <FormFieldSelect<AxisConfiguration>
          label="Select a filter"
          name="groupByFilter"
          options={categories}
          order={[]}
        />
      ),
    });

    return options;
  }, [meta.filters]);

  // TODO EES-721: Figure out how we should sort data
  // const sortOptions = useMemo<SelectOption[]>(() => {
  //   return [
  //     {
  //       label: 'Default',
  //       value: 'name',
  //     },
  //   ];
  // }, []);

  const limitOptions = useMemo<SelectOption[]>(() => {
    if (type !== 'major') {
      return [];
    }

    return dataSetCategories.map(({ filter }, index) => ({
      label: filter.label,
      value: index.toString(),
    }));
  }, [dataSetCategories, type]);

  const normalizeValues = useCallback(
    (values: FormValues): AxisConfiguration => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      const result = merge({}, axisConfiguration, values, {
        // `configuration.type` may be incorrectly set by
        // seeded releases, so we want to make sure this is
        // set using the `type` prop (which uses the axis key)
        type,
        // Numeric values may be treated as strings by Formik
        // due to the way they are encoded in the form input value.
        min: parseNumber(values.min),
        max: parseNumber(values.max),
        size: parseNumber(values.size),
        tickSpacing: parseNumber(values.tickSpacing),
      });
      // referenceLines are removable, so don't merge - update instead
      result.referenceLines = [...(values.referenceLines ?? [])];
      return result;
    },
    [axisConfiguration, type],
  );

  const handleFormChange = useCallback(
    ({ isValid, ...values }: FormValues & { isValid: boolean }) => {
      if (isValid) {
        onChange(normalizeValues(values));
      }
    },
    [normalizeValues, onChange],
  );

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    let schema: ObjectSchema<FormValues> = Yup.object({
      size: Yup.number().positive('Size of axis must be positive'),
      tickConfig: Yup.string().oneOf<TickConfig>(
        ['default', 'startEnd', 'custom'],
        'Select a valid tick display type',
      ),
      tickSpacing: Yup.number().when('tickConfig', {
        is: 'custom',
        then: Yup.number()
          .required('Enter tick spacing')
          .positive('Tick spacing must be positive'),
      }),
      max: Yup.number(),
      min: Yup.number(),
      visible: Yup.boolean(),
      unit: Yup.string(),
    });

    if (type === 'major' && !definition.axes.major?.constants?.groupBy) {
      schema = schema.shape({
        groupBy: Yup.string().oneOf<AxisGroupBy>(
          ['locations', 'timePeriod', 'filters', 'indicators'],
          'Choose a valid group by',
        ),
        groupByFilter: Yup.string(),
      });
    }

    if (axisDefinition?.capabilities.canRotateLabel) {
      schema = schema.shape({
        label: Yup.object<Label>({
          text: Yup.string(),
          rotated: Yup.boolean(),
          width: Yup.number().positive('Label width must be positive'),
        }),
      });
    } else {
      schema = schema.shape({
        label: Yup.object<Label>({
          text: Yup.string(),
          width: Yup.number().positive('Label width must be positive'),
        }),
      });
    }

    if (capabilities.canSort) {
      schema = schema.shape({
        sortAsc: Yup.boolean(),
      });
    }

    if (capabilities.hasGridLines) {
      schema = schema.shape({
        showGrid: Yup.boolean(),
      });
    }

    if (capabilities.hasReferenceLines) {
      schema = schema.shape({
        referenceLines: Yup.array(),
      });
    }

    return schema;
  }, [
    axisDefinition,
    capabilities.canSort,
    capabilities.hasGridLines,
    capabilities.hasReferenceLines,
    definition.axes.major,
    type,
  ]);

  const initialValues = useMemo<FormValues>(
    () => pick(axisConfiguration, Object.keys(validationSchema.fields)),
    [axisConfiguration, validationSchema.fields],
  );

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      const nextConfiguration = normalizeValues(values);

      if (nextConfiguration.groupBy) {
        const groupByFilters = getGroupByFilters(
          nextConfiguration.groupBy,
          meta,
        );

        // Strip out any reference lines that don't match. For the user's
        // convenience, we only do this when the chart is saved so that
        // they don't lose reference lines when toggling the `groupBy`.
        onSubmit({
          ...nextConfiguration,
          referenceLines: nextConfiguration.referenceLines.filter(line =>
            groupByFilters.some(filter => {
              if (filter instanceof LocationFilter) {
                return LocationFilter.createId(filter) === line.position;
              }
              return filter.value === line.position;
            }),
          ),
        });
      } else {
        onSubmit(nextConfiguration);
      }

      await submitForms();
    },
    [meta, normalizeValues, onSubmit, submitForms],
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={initialValues}
      initialTouched={
        hasSubmitted
          ? mapValues(validationSchema.fields, () => true)
          : undefined
      }
      validateOnMount
      validationSchema={validationSchema}
      onSubmit={handleSubmit}
    >
      {form => (
        <Form id={id}>
          <Effect
            value={{
              ...form.values,
              isValid: form.isValid,
            }}
            onChange={handleFormChange}
          />

          <Effect
            value={{
              formKey: type,
              isValid: form.isValid,
              submitCount: form.submitCount,
            }}
            onMount={updateForm}
            onChange={updateForm}
          />

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-half govuk-!-margin-bottom-6">
              <FormFieldset id="general" legend="General" legendSize="s">
                {validationSchema.fields.size && (
                  <FormFieldNumberInput<AxisConfiguration>
                    name="size"
                    min={0}
                    label="Size of axis (pixels)"
                    width={3}
                  />
                )}

                {validationSchema.fields.showGrid && (
                  <FormFieldCheckbox<AxisConfiguration>
                    name="showGrid"
                    label="Show grid lines"
                  />
                )}

                {validationSchema.fields.visible && (
                  <FormFieldCheckbox<AxisConfiguration>
                    name="visible"
                    label="Show axis"
                    conditional={
                      <>
                        <FormFieldTextInput<AxisConfiguration>
                          label="Displayed unit"
                          name="unit"
                          hint="Leave blank to set default from metadata"
                          width={10}
                        />
                      </>
                    }
                  />
                )}

                {validationSchema.fields.groupBy && (
                  <FormFieldRadioGroup<AxisConfiguration>
                    legend="Group data by"
                    legendSize="s"
                    name="groupBy"
                    options={groupByOptions}
                    onChange={e => {
                      if (e.target.value !== 'filters') {
                        form.setFieldValue('groupByFilter', '');
                      }
                    }}
                  />
                )}

                <FormFieldset id="labels" legend="Labels" legendSize="s">
                  <FormFieldTextInput label="Label" name="label.text" />

                  <FormFieldNumberInput
                    label="Width (pixels)"
                    name="label.width"
                    width={5}
                    min={0}
                  />

                  {(validationSchema.fields.label as ObjectSchema<Label>).fields
                    .rotated && (
                    <FormFieldCheckbox
                      name="label.rotated"
                      label="Rotate 90 degrees"
                    />
                  )}
                </FormFieldset>
              </FormFieldset>
            </div>

            <div className="govuk-grid-column-one-half govuk-!-margin-bottom-6">
              {validationSchema.fields.sortAsc && (
                <FormFieldset id="sort" legend="Sorting" legendSize="s">
                  {/* <FormFieldSelect<AxisConfiguration>*/}
                  {/*  id={`${id}-sortBy`}*/}
                  {/*  name="sortBy"*/}
                  {/*  label="Sort data by"*/}
                  {/*  options={sortOptions}*/}
                  {/* />*/}
                  <FormFieldCheckbox<AxisConfiguration>
                    name="sortAsc"
                    label="Sort ascending"
                  />
                </FormFieldset>
              )}

              {validationSchema.fields.tickConfig && (
                <FormFieldRadioGroup<AxisConfiguration>
                  name="tickConfig"
                  legend="Tick display type"
                  legendSize="s"
                  order={[]}
                  options={[
                    {
                      value: 'default',
                      label: 'Automatic',
                    },
                    {
                      label: 'Start and end only',
                      value: 'startEnd',
                    },
                    {
                      label: 'Custom',
                      value: 'custom',
                      conditional: (
                        <FormFieldNumberInput<AxisConfiguration>
                          name="tickSpacing"
                          width={10}
                          label="Every nth value"
                        />
                      ),
                    },
                  ]}
                />
              )}

              {type === 'minor' &&
                (validationSchema.fields.min ||
                  validationSchema.fields.max) && (
                  <FormFieldset
                    id="minorAxisRange"
                    legend="Axis range"
                    legendSize="s"
                    hint="Leaving these values blank will set them to 'auto'"
                  >
                    <div className={styles.axisRange}>
                      {validationSchema.fields.min && (
                        <FormFieldNumberInput<AxisConfiguration>
                          name="min"
                          width={10}
                          label="Minimum value"
                          formGroupClass="govuk-!-margin-right-2"
                        />
                      )}
                      {validationSchema.fields.max && (
                        <FormFieldNumberInput<AxisConfiguration>
                          name="max"
                          width={10}
                          label="Maximum value"
                        />
                      )}
                    </div>
                  </FormFieldset>
                )}

              {type === 'major' &&
                (validationSchema.fields.min ||
                  validationSchema.fields.max) && (
                  <FormFieldset
                    id="majorAxisRange"
                    legend="Axis range"
                    legendSize="s"
                  >
                    {validationSchema.fields.min && (
                      <FormFieldSelect<AxisConfiguration>
                        label="Minimum"
                        name="min"
                        placeholder="Default"
                        options={limitOptions}
                      />
                    )}
                    {validationSchema.fields.max && (
                      <FormFieldSelect<AxisConfiguration>
                        label="Maximum"
                        name="max"
                        placeholder="Default"
                        options={limitOptions}
                      />
                    )}
                  </FormFieldset>
                )}
            </div>
          </div>

          {validationSchema.fields.referenceLines && (
            <ChartReferenceLinesConfiguration
              axisDefinition={axisDefinition}
              dataSetCategories={dataSetCategories}
              lines={form.values.referenceLines ?? []}
              minorAxisDomain={minorAxisDomain}
              onAddLine={line => {
                form.setFieldValue('referenceLines', [
                  ...(form.values.referenceLines ?? []),
                  line,
                ]);
              }}
              onRemoveLine={line => {
                form.setFieldValue(
                  'referenceLines',
                  form.values.referenceLines?.filter(
                    refLine =>
                      refLine.position !== line.position &&
                      refLine.label !== line.label,
                  ),
                );
              }}
            />
          )}

          <ChartBuilderSaveActions
            formId={id}
            formKey={type}
            disabled={form.isSubmitting}
          >
            {buttons}
          </ChartBuilderSaveActions>
        </Form>
      )}
    </Formik>
  );
};

export default ChartAxisConfiguration;

function getGroupByFilters(groupBy: AxisGroupBy, meta: FullTableMeta) {
  switch (groupBy) {
    case 'filters':
      return Object.values(meta.filters).flatMap(
        filterGroup => filterGroup.options,
      );
    case 'indicators':
      return meta.indicators;
    case 'locations':
      return meta.locations;
    case 'timePeriod':
      return meta.timePeriodRange.map(timePeriod => ({
        value: `${timePeriod.year}_${timePeriod.code}`,
        label: timePeriod.label,
      }));
    default:
      return [];
  }
}
