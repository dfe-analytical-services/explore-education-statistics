import styles from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration.module.scss';
import ChartBuilderSaveActions from '@admin/pages/release/datablocks/components/chart/ChartBuilderSaveActions';
import { useChartBuilderFormsContext } from '@admin/pages/release/datablocks/components/chart/contexts/ChartBuilderFormsContext';
import Button from '@common/components/Button';
import Effect from '@common/components/Effect';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldSelect,
  FormFieldset,
  FormFieldTextInput,
  FormTextInput,
} from '@common/components/form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormNumberInput from '@common/components/form/FormNumberInput';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import {
  AxisConfiguration,
  AxisGroupBy,
  AxisType,
  ChartDefinition,
  Label,
  ReferenceLine,
  TickConfig,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { OmitStrict } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import merge from 'lodash/merge';
import pick from 'lodash/pick';
import React, { ReactNode, useCallback, useMemo, useState } from 'react';
import { ObjectSchema } from 'yup';

type FormValues = Partial<OmitStrict<AxisConfiguration, 'dataSets' | 'type'>>;

interface Props {
  buttons?: ReactNode;
  id: string;
  type: AxisType;
  configuration: AxisConfiguration;
  definition: ChartDefinition;
  data: TableDataResult[];
  meta: FullTableMeta;
  showGroupByFilter?: boolean;
  onChange: (configuration: AxisConfiguration) => void;
  onSubmit: (configuration: AxisConfiguration) => void;
}

const ChartAxisConfiguration = ({
  buttons,
  configuration,
  definition,
  id,
  data,
  meta,
  showGroupByFilter = false, // EES-2467 remove when BE done.
  type,
  onChange,
  onSubmit,
}: Props) => {
  const { capabilities } = definition;

  const { hasSubmitted, updateForm, submit } = useChartBuilderFormsContext();

  const dataSetCategories = useMemo<DataSetCategory[]>(() => {
    if (type === 'minor') {
      return [];
    }

    const config: AxisConfiguration = {
      ...configuration,
      min: 0,
      max: undefined,
    };

    return createDataSetCategories(config, data, meta);
  }, [configuration, data, meta, type]);

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

    if (canGroupByFilters) {
      // EES-2467 remove this check when BE done.
      if (showGroupByFilter) {
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
      } else {
        options.push({
          label: 'Filters',
          value: 'filters',
        });
      }
    }

    return options;
  }, [meta.filters, showGroupByFilter]);

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

  const [referenceLine, setReferenceLine] = useState<ReferenceLine>({
    position: '',
    label: '',
  });

  const referenceOptions = useMemo<SelectOption[]>(() => {
    if (configuration.groupBy) {
      const options: SelectOption[] = [];

      switch (configuration.groupBy) {
        case 'filters':
          options.push(
            ...Object.values(meta.filters).flatMap(
              filterGroup => filterGroup.options,
            ),
          );
          break;
        case 'indicators':
          options.push(...meta.indicators);
          break;
        case 'locations':
          options.push(...meta.locations);
          break;
        case 'timePeriod':
          options.push(
            ...meta.timePeriodRange.map(timePeriod => ({
              value: `${timePeriod.year}_${timePeriod.code}`,
              label: timePeriod.label,
            })),
          );
          break;
        default:
          break;
      }

      return options;
    }
    return [];
  }, [configuration.groupBy, meta]);

  const normalizeValues = useCallback(
    (values: FormValues): AxisConfiguration => {
      // Use `merge` as we want to avoid potential undefined
      // values from overwriting existing values
      const result = merge({}, configuration, values, {
        // `configuration.type` may be incorrectly set by
        // seeded releases so we want to make sure this is
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
    [configuration, type],
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
    const axisDefinition = definition.axes[type];

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
    capabilities.canSort,
    capabilities.hasGridLines,
    capabilities.hasReferenceLines,
    definition.axes,
    type,
  ]);

  const initialValues = useMemo<FormValues>(
    () => pick(configuration, Object.keys(validationSchema.fields)),
    [configuration, validationSchema.fields],
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
      onSubmit={values => {
        onSubmit(normalizeValues(values));
        submit();
      }}
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
                    label="Size of axis (px)"
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
                  />
                )}

                <FormFieldset id="labels" legend="Labels" legendSize="s">
                  <FormFieldTextInput label="Label" name="label.text" />

                  <FormFieldNumberInput
                    label="Width (px)"
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
            <table className="govuk-table">
              <caption className="govuk-heading-s">Reference lines</caption>
              <thead>
                <tr>
                  <th>Position</th>
                  <th>Label</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {form.values.referenceLines &&
                  form.values.referenceLines.map((refLine, idx) => (
                    <tr key={`${refLine.label}_${refLine.position}`}>
                      <td>{refLine.position}</td>
                      <td>{refLine.label}</td>
                      <td>
                        <button
                          className="govuk-button govuk-button--secondary govuk-!-margin-0"
                          type="button"
                          onClick={() => {
                            const newReferenceLines = [
                              ...(form.values.referenceLines ?? []),
                            ];
                            newReferenceLines.splice(idx, 1);

                            form.setFieldValue(
                              'referenceLines',
                              newReferenceLines,
                            );
                          }}
                        >
                          Remove
                        </button>
                      </td>
                    </tr>
                  ))}
                <tr>
                  <td>
                    {type === 'minor' && (
                      <FormNumberInput
                        name={`referenceLines[${form.values.referenceLines?.length}].position`}
                        id={`${id}-referenceLines-position`}
                        label="Position"
                        hideLabel
                        value={referenceLine.position as number}
                        onChange={e => {
                          setReferenceLine({
                            ...referenceLine,
                            position: e.target.value,
                          });
                        }}
                      />
                    )}
                    {type === 'major' && (
                      <FormSelect
                        name={`referenceLines[${form.values.referenceLines?.length}].position`}
                        id={`${id}-referenceLines-position`}
                        label="Position"
                        hideLabel
                        value={referenceLine.position?.toString()}
                        placeholder="Select position"
                        order={FormSelect.unordered}
                        options={referenceOptions}
                        onChange={e => {
                          setReferenceLine({
                            ...referenceLine,
                            position: e.target.value,
                          });
                        }}
                      />
                    )}
                  </td>
                  <td>
                    <FormTextInput
                      name={`referenceLines[${form.values.referenceLines?.length}].label`}
                      id={`${id}-referenceLines-label`}
                      label="Label"
                      hideLabel
                      value={referenceLine.label}
                      onChange={e => {
                        setReferenceLine({
                          ...referenceLine,
                          label: e.target.value,
                        });
                      }}
                    />
                  </td>
                  <td>
                    <Button
                      disabled={
                        referenceLine.position === '' ||
                        referenceLine.label === ''
                      }
                      className="govuk-!-margin-bottom-0 dfe-float--right"
                      onClick={() => {
                        form.setFieldValue('referenceLines', [
                          ...(form.values.referenceLines ?? []),
                          referenceLine,
                        ]);

                        setReferenceLine({ label: '', position: '' });
                      }}
                    >
                      Add line
                    </Button>
                  </td>
                </tr>
              </tbody>
            </table>
          )}

          <ChartBuilderSaveActions formId={id} formKey={type}>
            {buttons}
          </ChartBuilderSaveActions>
        </Form>
      )}
    </Formik>
  );
};

export default ChartAxisConfiguration;
