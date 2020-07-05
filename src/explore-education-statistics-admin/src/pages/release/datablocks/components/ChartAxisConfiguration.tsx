import styles from '@admin/pages/release/datablocks/components/ChartAxisConfiguration.module.scss';
import { ChartBuilderForm } from '@admin/pages/release/datablocks/components/ChartBuilder';
import ChartBuilderSaveButton from '@admin/pages/release/datablocks/components/ChartBuilderSaveButton';
import { FormState } from '@admin/pages/release/datablocks/reducers/chartBuilderReducer';
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

import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import {
  AxisConfiguration,
  AxisGroupBy,
  AxisType,
  ChartDefinition,
  Label,
  ReferenceLine,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { Dictionary, OmitStrict } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import mapValues from 'lodash/mapValues';
import merge from 'lodash/merge';
import pick from 'lodash/pick';
import React, { ReactNode, useCallback, useMemo, useState } from 'react';
import { ObjectSchema, Schema } from 'yup';

type FormValues = Partial<OmitStrict<AxisConfiguration, 'dataSets' | 'type'>>;

interface Props {
  buttons?: ReactNode;
  canSaveChart: boolean;
  defaultDataType?: AxisGroupBy;
  forms: Dictionary<ChartBuilderForm>;
  hasSubmittedChart: boolean;
  id: string;
  type: AxisType;
  configuration: AxisConfiguration;
  definition: ChartDefinition;
  data: TableDataResult[];
  meta: FullTableMeta;
  onChange: (configuration: AxisConfiguration) => void;
  onFormStateChange: (state: { form: AxisType } & FormState) => void;
  onSubmit: (configuration: AxisConfiguration) => void;
}

const ChartAxisConfiguration = ({
  buttons,
  canSaveChart,
  configuration,
  definition,
  forms,
  hasSubmittedChart,
  id,
  data,
  meta,
  type,
  onChange,
  onFormStateChange,
  onSubmit,
}: Props) => {
  const { capabilities } = definition;

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

  const groupByOptions = useMemo<SelectOption<AxisGroupBy>[]>(() => {
    const options: SelectOption<AxisGroupBy>[] = [
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
      options.push({
        label: 'Filters',
        value: 'filters',
      });
    }

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
      return merge({}, configuration, values, {
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
      size: Yup.number()
        .required('Enter size of axis')
        .positive('Size of axis must be positive')
        .max(100, 'Size of axis must be less than 100px'),
      tickConfig: Yup.string().oneOf(
        ['default', 'startEnd', 'custom'],
        'Select a valid tick display type',
      ) as Schema<AxisConfiguration['tickConfig']>,
      tickSpacing: Yup.number().when('tickConfig', {
        is: 'custom',
        then: Yup.number()
          .required('Enter tick spacing')
          .positive('Tick spacing must be positive'),
      }),
      max: Yup.number(),
      min: Yup.number(),
      visible: Yup.boolean(),
    });

    if (type === 'major' && !capabilities.fixedAxisGroupBy) {
      schema = schema.shape({
        groupBy: Yup.string().oneOf([
          'locations',
          'timePeriod',
          'filters',
          'indicators',
        ]) as Schema<AxisConfiguration['groupBy']>,
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

    if (capabilities.gridLines) {
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
    capabilities.fixedAxisGroupBy,
    capabilities.gridLines,
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
              form: type,
              isValid: form.isValid,
              submitCount: form.submitCount,
            }}
            onMount={onFormStateChange}
            onChange={onFormStateChange}
          />

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-half govuk-!-margin-bottom-6">
              <FormFieldset
                id={`${id}-general`}
                legend="General"
                legendSize="s"
              >
                {validationSchema.fields.size && (
                  <FormFieldNumberInput<AxisConfiguration>
                    id={`${id}-size`}
                    name="size"
                    min={0}
                    max={100}
                    label="Size of axis (px)"
                    width={3}
                  />
                )}

                {validationSchema.fields.groupBy && (
                  <FormFieldSelect<AxisConfiguration>
                    id={`${id}-groupBy`}
                    label="Group data by"
                    name="groupBy"
                    options={groupByOptions}
                  />
                )}

                {validationSchema.fields.showGrid && (
                  <FormFieldCheckbox<AxisConfiguration>
                    id={`${id}-showGrid`}
                    name="showGrid"
                    label="Show grid lines"
                  />
                )}

                {validationSchema.fields.visible && (
                  <FormFieldCheckbox<AxisConfiguration>
                    id={`${id}-visible`}
                    name="visible"
                    label="Show axis"
                    conditional={
                      <>
                        <FormFieldTextInput<AxisConfiguration>
                          id={`${id}-unit`}
                          label="Displayed unit"
                          name="unit"
                          hint="Leave blank to set default from metadata"
                          width={10}
                        />
                      </>
                    }
                  />
                )}

                <FormFieldset id={`${id}-axis`} legend="Labels" legendSize="s">
                  <FormFieldTextInput
                    id={`${id}-label-text`}
                    label="Label"
                    name="label.text"
                  />

                  <FormFieldNumberInput
                    id={`${id}-label-width`}
                    label="Width (px)"
                    name="label.width"
                    width={5}
                    min={0}
                  />

                  {(validationSchema.fields.label as ObjectSchema<Label>).fields
                    .rotated && (
                    <FormFieldCheckbox
                      id={`${id}-label-rotated`}
                      name="label.rotated"
                      label="Rotate 90 degrees"
                    />
                  )}
                </FormFieldset>
              </FormFieldset>
            </div>

            <div className="govuk-grid-column-one-half govuk-!-margin-bottom-6">
              {validationSchema.fields.sortAsc && (
                <FormFieldset id={`${id}-sort`} legend="Sorting" legendSize="s">
                  {/* <FormFieldSelect<AxisConfiguration>*/}
                  {/*  id={`${id}-sortBy`}*/}
                  {/*  name="sortBy"*/}
                  {/*  label="Sort data by"*/}
                  {/*  options={sortOptions}*/}
                  {/* />*/}
                  <FormFieldCheckbox<AxisConfiguration>
                    id={`${id}-sortAsc`}
                    name="sortAsc"
                    label="Sort ascending"
                  />
                </FormFieldset>
              )}

              {validationSchema.fields.tickConfig && (
                <FormFieldRadioGroup<AxisConfiguration>
                  id={`${id}-tickConfig`}
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
                          id={`${id}-tickSpacing`}
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
                    id={`${id}-minorAxisRange`}
                    legend="Axis range"
                    legendSize="s"
                    hint="Leaving these values blank will set them to 'auto'"
                  >
                    <div className={styles.axisRange}>
                      {validationSchema.fields.min && (
                        <FormFieldNumberInput<AxisConfiguration>
                          id={`${id}-minorMin`}
                          name="min"
                          width={10}
                          label="Minimum value"
                          formGroupClass="govuk-!-margin-right-2"
                        />
                      )}
                      {validationSchema.fields.max && (
                        <FormFieldNumberInput<AxisConfiguration>
                          id={`${id}-minorMax`}
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
                    id={`${id}-majorAxisRange`}
                    legend="Axis range"
                    legendSize="s"
                  >
                    {validationSchema.fields.min && (
                      <FormFieldSelect<AxisConfiguration>
                        id={`${id}-majorMin`}
                        label="Minimum"
                        name="min"
                        placeholder="Default"
                        options={limitOptions}
                      />
                    )}
                    {validationSchema.fields.max && (
                      <FormFieldSelect<AxisConfiguration>
                        id={`${id}-majorMax`}
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
                              ...(form.values.referenceLines || []),
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
                          ...(form.values.referenceLines || []),
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

          <ChartBuilderSaveButton
            formId={id}
            forms={forms}
            showSubmitError={
              form.isValid && form.submitCount > 0 && !canSaveChart
            }
          />

          {buttons}
        </Form>
      )}
    </Formik>
  );
};

export default ChartAxisConfiguration;
