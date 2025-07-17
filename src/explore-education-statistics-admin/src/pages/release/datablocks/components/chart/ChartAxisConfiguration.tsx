import styles from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration.module.scss';
import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import ChartReferenceLinesConfiguration from '@admin/pages/release/datablocks/components/chart/ChartReferenceLinesConfiguration';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import Effect from '@common/components/Effect';
import { FormFieldset } from '@common/components/form';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import { SelectOption } from '@common/components/form/FormSelect';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import {
  AxesConfiguration,
  AxisConfiguration,
  AxisGroupBy,
  AxisType,
  ChartDefinition,
  ReferenceLine,
  ReferenceLineStyle,
  TickConfig,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import {
  otherAxisPositionTypes,
  OtherAxisPositionType,
} from '@common/modules/charts/types/referenceLinePosition';
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
import isEqual from 'lodash/isEqual';
import merge from 'lodash/merge';
import omit from 'lodash/omit';
import pick from 'lodash/pick';
import React, {
  ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useRef,
} from 'react';
import { ObjectSchema } from 'yup';

interface FormReferenceLine extends ReferenceLine {
  otherAxisPositionType?: OtherAxisPositionType;
}

export interface ChartAxisConfigurationFormValues
  extends OmitStrict<
    AxisConfiguration,
    'dataSets' | 'type' | 'label' | 'referenceLines'
  > {
  labelText?: string;
  labelWidth?: number | null;
  referenceLines?: FormReferenceLine[];
}

interface Props {
  axesConfiguration: AxesConfiguration;
  buttons?: ReactNode;
  definition: ChartDefinition;
  data: TableDataResult[];
  id: string;
  includeNonNumericData?: boolean;
  meta: FullTableMeta;
  stacked?: boolean;
  type: AxisType;
  onChange: (configuration: AxisConfiguration) => void;
  onSubmit: (configuration: AxisConfiguration) => void;
}

const ChartAxisConfiguration = ({
  axesConfiguration,
  buttons,
  definition,
  id,
  includeNonNumericData,
  data,
  meta,
  stacked,
  type,
  onChange,
  onSubmit,
}: Props) => {
  const { updateForm, submitForms } = useChartBuilderFormsContext();

  const { axes, capabilities } = definition;
  const axisDefinition = axes[type];
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

    return createDataSetCategories({
      axisConfiguration: config,
      data,
      meta,
      includeNonNumericData,
    });
  }, [axesConfiguration.major, data, meta, includeNonNumericData]);

  const chartData = dataSetCategories.map(toChartData);
  const minorAxisDomain = axesConfiguration.minor
    ? calculateMinorAxisDomainValues(chartData, axesConfiguration.minor)
    : undefined;

  const isGroupedByFilterWithGroups = useMemo(() => {
    if (!stacked || axisConfiguration?.groupBy !== 'filters') {
      return false;
    }

    const filters = Object.values(meta.filters);
    const groupedByName =
      axisConfiguration.groupByFilter ??
      (filters.length === 1 ? filters[0].name : undefined);

    return filters
      .find(filter => filter.name === groupedByName)
      ?.options.some(item => item.group !== 'Default');
  }, [
    axisConfiguration?.groupBy,
    axisConfiguration?.groupByFilter,
    meta.filters,
    stacked,
  ]);

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
        <>
          <FormFieldSelect<AxisConfiguration>
            label="Select a filter"
            name="groupByFilter"
            options={categories}
            order={[]}
          />
          {isGroupedByFilterWithGroups && (
            <FormFieldCheckbox<ChartAxisConfigurationFormValues>
              name="groupByFilterGroups"
              label="Group by filter groups"
            />
          )}
        </>
      ),
    });

    return options;
  }, [isGroupedByFilterWithGroups, meta.filters]);

  const limitOptions = useMemo<SelectOption[]>(() => {
    if (type === 'minor') {
      return [];
    }

    return dataSetCategories.map(({ filter }, index) => ({
      label: filter.label,
      value: index.toString(),
    }));
  }, [dataSetCategories, type]);

  const normalizeValues = useCallback(
    (values: ChartAxisConfigurationFormValues): AxisConfiguration => {
      const refLines = (values.referenceLines ?? []).map(line =>
        omit(line, 'otherAxisPositionType'),
      );
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      const result = merge({}, axisConfiguration, values, {
        // `configuration.type` may be incorrectly set by
        // seeded releases, so we want to make sure this is
        // set using the `type` prop (which uses the axis key)
        // type,
        label: {
          text: values.labelText,
          width: values.labelWidth,
        },
        // These are strings when set by Selects
        min: parseNumber(values.min),
        max: parseNumber(values.max),
      });
      // referenceLines are removable, so don't merge - update instead
      result.referenceLines = [...refLines];
      result.decimalPlaces = values.decimalPlaces;

      return omit(result, ['labelText', 'labelWidth']);
    },
    [axisConfiguration],
  );

  const handleFormChange = useCallback(
    ({ ...values }: ChartAxisConfigurationFormValues) => {
      onChange(normalizeValues(values));
    },
    [normalizeValues, onChange],
  );

  const validationSchema = useMemo<
    ObjectSchema<Partial<ChartAxisConfigurationFormValues>>
  >(() => {
    let schema = Yup.object<ChartAxisConfigurationFormValues>({
      size: Yup.number().positive('Size of axis must be positive'),
      tickConfig: Yup.string().oneOf<TickConfig>(
        [
          'default',
          'startEnd',
          'custom',
          ...(type === 'major' && capabilities.canShowAllMajorAxisTicks
            ? ['showAll' as TickConfig]
            : []),
        ],
        'Select a valid tick display type',
      ),
      tickSpacing: Yup.number().when('tickConfig', {
        is: 'custom',
        then: s =>
          s
            .required('Enter tick spacing')
            .positive('Tick spacing must be positive'),
        otherwise: s => s.notRequired(),
      }),
      max: Yup.number(),
      min: Yup.number(),
      visible: Yup.boolean(),
      unit: Yup.string(),
      decimalPlaces: Yup.number().min(
        0,
        'Displayed decimal places must be positive',
      ),
      labelText: Yup.string(),
      labelWidth: Yup.number().positive('Label width must be positive'),
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
        referenceLines: Yup.array()
          .defined()
          .of(
            Yup.object({
              label: Yup.string().required('Enter label'),
              labelWidth: Yup.number().optional(),
              position: Yup.mixed<string | number>()
                .required()
                .transform(value => (!value ? undefined : value))
                .required('Enter position')
                .test({
                  name: 'axisPosition',
                  message: `Enter a position within the ${
                    axisDefinition?.axis === 'x' ? 'X' : 'Y'
                  } axis min/max range`,
                  test: value => {
                    if (typeof value !== 'number') {
                      return true;
                    }
                    return type === 'minor' && minorAxisDomain
                      ? value >= minorAxisDomain?.min &&
                          value <= minorAxisDomain.max
                      : true;
                  },
                }),
              style: Yup.string()
                .required('Enter style')
                .oneOf<ReferenceLineStyle>(['dashed', 'solid', 'none']),
              otherAxisEnd: Yup.string()
                .optional()
                .when(['otherAxisPositionType', 'position'], {
                  is: (
                    otherAxisPositionType: OtherAxisPositionType,
                    position: string | number,
                  ) =>
                    otherAxisPositionType ===
                      otherAxisPositionTypes.betweenDataPoints ||
                    position === otherAxisPositionTypes.betweenDataPoints,
                  then: s =>
                    s.required('Enter end point').min(1, 'Enter end point'),
                  otherwise: s => s.notRequired(),
                })
                .test(
                  'otherAxisEnd',
                  'End point cannot match start point',
                  function checkOtherAxisEnd(value) {
                    /* eslint-disable react/no-this-in-sfc */
                    if (this.parent.otherAxisStart) {
                      return value !== this.parent.otherAxisStart;
                    }
                    return true;
                    /* eslint-enable react/no-this-in-sfc */
                  },
                ),
              otherAxisStart: Yup.string()
                .optional()
                .when(['otherAxisPositionType', 'position'], {
                  is: (
                    otherAxisPositionType: OtherAxisPositionType,
                    position: string | number,
                  ) =>
                    otherAxisPositionType ===
                      otherAxisPositionTypes.betweenDataPoints ||
                    position === otherAxisPositionTypes.betweenDataPoints,
                  then: s =>
                    s.required('Enter start point').min(1, 'Enter start point'),
                  otherwise: s => s.notRequired(),
                }),

              otherAxisPosition:
                type === 'minor'
                  ? Yup.number().when('otherAxisPositionType', {
                      is: otherAxisPositionTypes.custom,
                      then: s =>
                        s
                          .required('Enter a percentage between 0 and 100%')
                          .test({
                            name: 'otherAxisPosition',
                            message: 'Enter a percentage between 0 and 100%',
                            test: value => {
                              if (typeof value !== 'number') {
                                return true;
                              }
                              return value >= 0 && value <= 100;
                            },
                          }),
                      otherwise: s => s.notRequired(),
                    })
                  : Yup.number()
                      .when('position', {
                        is: otherAxisPositionTypes.betweenDataPoints,
                        then: s =>
                          s.required(
                            `Enter a ${
                              axisDefinition?.axis === 'x' ? 'Y' : 'X'
                            } axis position`,
                          ),
                      })
                      .test({
                        name: 'otherAxisPosition',
                        message: `Enter a position within the ${
                          axisDefinition?.axis === 'x' ? 'Y' : 'X'
                        } axis min/max range`,
                        test: value => {
                          if (typeof value !== 'number') {
                            return true;
                          }

                          return minorAxisDomain
                            ? value >= minorAxisDomain?.min &&
                                value <= minorAxisDomain.max
                            : true;
                        },
                      }),
              otherAxisPositionType: Yup.string()
                .optional()
                .oneOf<OtherAxisPositionType>(
                  Object.values(otherAxisPositionTypes),
                ),
            }),
          ),
      });
    }

    return schema;
  }, [
    axisDefinition?.axis,
    capabilities.canShowAllMajorAxisTicks,
    capabilities.canSort,
    capabilities.hasGridLines,
    capabilities.hasReferenceLines,
    definition.axes.major?.constants?.groupBy,
    minorAxisDomain,
    type,
  ]);

  const getInitialValues = useCallback(
    (values?: AxisConfiguration): ChartAxisConfigurationFormValues => {
      const initialValues = {
        ...pick(values, Object.keys(validationSchema.fields)),
        groupByFilterGroups:
          type === 'major' && isGroupedByFilterWithGroups && stacked
            ? values?.groupByFilterGroups
            : undefined,
        max:
          type === 'major' && !values?.max
            ? parseNumber(limitOptions[limitOptions.length - 1]?.value)
            : values?.max,
        labelText: values?.label?.text ?? '',
        labelWidth: values?.label?.width ?? undefined,
        decimalPlaces: values?.decimalPlaces ?? undefined,
        referenceLines:
          values?.referenceLines.map(line => {
            return {
              ...line,
              otherAxisPositionType:
                type === 'minor' ? getOtherAxisPositionType(line) : undefined,
              style:
                line?.style ??
                axisDefinition?.referenceLineDefaults?.style ??
                'dashed',
            };
          }) ?? [],
      };

      return initialValues;
    },
    [
      axisDefinition?.referenceLineDefaults?.style,
      isGroupedByFilterWithGroups,
      limitOptions,
      stacked,
      type,
      validationSchema.fields,
    ],
  );

  const chartType = useRef(definition.type);
  const initialAxisConfiguration = useRef(getInitialValues(axisConfiguration));
  const initialDataSets = useRef(axisConfiguration?.dataSets);

  // Update initial options if the chart definition or data sets change
  useEffect(() => {
    if (definition.type !== chartType.current) {
      initialAxisConfiguration.current = getInitialValues(axisConfiguration);
      chartType.current = definition.type;
    }
    if (!isEqual(axisConfiguration?.dataSets, initialDataSets.current)) {
      initialDataSets.current = axisConfiguration?.dataSets;
    }
  }, [dataSetCategories, definition.type, axisConfiguration, getInitialValues]);

  const handleSubmit = useCallback(
    async (values: ChartAxisConfigurationFormValues) => {
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
          referenceLines: nextConfiguration.referenceLines.filter(
            line =>
              line.position === otherAxisPositionTypes.betweenDataPoints ||
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
    <FormProvider
      enableReinitialize
      initialValues={initialAxisConfiguration.current}
      validationSchema={validationSchema}
    >
      {({ formState, setValue, watch }) => {
        const values = watch();

        return (
          <Form id={id} onSubmit={handleSubmit}>
            <Effect
              value={{
                ...omit(values, 'referenceLines'),
              }}
              onChange={() => handleFormChange(values)}
            />

            <Effect
              value={{
                formKey: type,
                isValid: formState.isValid,
                submitCount: formState.submitCount,
              }}
              onMount={updateForm}
              onChange={updateForm}
            />

            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-half govuk-!-margin-bottom-6">
                <FormFieldset id="general" legend="General" legendSize="s">
                  {validationSchema.fields.size && (
                    <FormFieldNumberInput<ChartAxisConfigurationFormValues>
                      name="size"
                      min={0}
                      label="Size of axis (pixels)"
                      width={3}
                    />
                  )}

                  {validationSchema.fields.showGrid && (
                    <FormFieldCheckbox<ChartAxisConfigurationFormValues>
                      name="showGrid"
                      label="Show grid lines"
                    />
                  )}

                  {validationSchema.fields.visible && (
                    <FormFieldCheckbox<ChartAxisConfigurationFormValues>
                      name="visible"
                      label="Show axis"
                      conditional={
                        <>
                          <FormFieldTextInput<ChartAxisConfigurationFormValues>
                            label="Displayed unit"
                            name="unit"
                            hint="Leave blank to set default from metadata"
                            width={10}
                          />
                          {type === 'minor' && (
                            <FormFieldNumberInput<ChartAxisConfigurationFormValues>
                              label="Displayed decimal places"
                              name="decimalPlaces"
                              hint="Leave blank to set default from metadata"
                              width={10}
                              min={0}
                            />
                          )}
                        </>
                      }
                    />
                  )}

                  {validationSchema.fields.groupBy && (
                    <FormFieldRadioGroup<ChartAxisConfigurationFormValues>
                      legend="Group data by"
                      legendSize="s"
                      name="groupBy"
                      options={groupByOptions}
                      onChange={e => {
                        if (e.target.value !== 'filters') {
                          setValue('groupByFilter' as const, '');
                        }
                      }}
                    />
                  )}

                  <FormFieldset id="labels" legend="Labels" legendSize="s">
                    <FormFieldTextInput label="Label" name="labelText" />

                    <FormFieldNumberInput
                      label="Width (pixels)"
                      name="labelWidth"
                      width={5}
                      min={0}
                    />
                  </FormFieldset>
                </FormFieldset>
              </div>

              <div className="govuk-grid-column-one-half govuk-!-margin-bottom-6">
                {validationSchema.fields.sortAsc && (
                  <FormFieldset id="sort" legend="Sorting" legendSize="s">
                    <FormFieldCheckbox<ChartAxisConfigurationFormValues>
                      name="sortAsc"
                      label="Sort ascending"
                    />
                  </FormFieldset>
                )}

                {validationSchema.fields.tickConfig && (
                  <FormFieldRadioGroup<ChartAxisConfigurationFormValues>
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
                      ...(type === 'major' &&
                      capabilities.canShowAllMajorAxisTicks
                        ? [
                            {
                              label: 'Show all',
                              value: 'showAll',
                            },
                          ]
                        : []),
                      {
                        label: 'Custom',
                        value: 'custom',
                        conditional: (
                          <FormFieldNumberInput<ChartAxisConfigurationFormValues>
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
                          <FormFieldNumberInput<ChartAxisConfigurationFormValues>
                            name="min"
                            width={10}
                            label="Minimum value"
                            formGroupClass="govuk-!-margin-right-2"
                          />
                        )}
                        {validationSchema.fields.max && (
                          <FormFieldNumberInput<ChartAxisConfigurationFormValues>
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
                        <FormFieldSelect<ChartAxisConfigurationFormValues>
                          isNumberField
                          label="Minimum"
                          name="min"
                          options={limitOptions}
                        />
                      )}
                      {validationSchema.fields.max && (
                        <FormFieldSelect<ChartAxisConfigurationFormValues>
                          isNumberField
                          label="Maximum"
                          name="max"
                          options={limitOptions}
                        />
                      )}
                    </FormFieldset>
                  )}
              </div>
            </div>
            {capabilities.hasReferenceLines && (
              <ChartReferenceLinesConfiguration
                axisDefinition={axisDefinition}
                chartType={chartType.current}
                dataSetCategories={dataSetCategories}
                referenceLines={
                  initialAxisConfiguration.current.referenceLines ?? []
                }
                onSubmit={updatedReferenceLines => {
                  setValue('referenceLines' as const, updatedReferenceLines);
                  handleFormChange({
                    ...axisConfiguration,
                    referenceLines: updatedReferenceLines,
                  });
                }}
              />
            )}

            <ChartBuilderSaveActions
              formId={id}
              formKey={type}
              disabled={formState.isSubmitting}
            >
              {buttons}
            </ChartBuilderSaveActions>
          </Form>
        );
      }}
    </FormProvider>
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

function getOtherAxisPositionType(
  referenceLine?: ReferenceLine,
): OtherAxisPositionType {
  if (referenceLine?.otherAxisEnd && referenceLine?.otherAxisStart) {
    return otherAxisPositionTypes.betweenDataPoints;
  }
  if (referenceLine?.otherAxisPosition) {
    return otherAxisPositionTypes.custom;
  }
  return otherAxisPositionTypes.default;
}
