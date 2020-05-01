import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import Button from '@common/components/Button';
import Effect from '@common/components/Effect';
import ErrorSummary from '@common/components/ErrorSummary';
import {
  Form,
  FormFieldRadioGroup,
  FormFieldSelect,
  FormFieldset,
  FormFieldTextInput,
  FormGroup,
  Formik,
  FormTextInput,
} from '@common/components/form';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';

import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import {
  AxisConfiguration,
  AxisGroupBy,
  AxisType,
  ChartCapabilities,
  ReferenceLine,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import { OmitStrict } from '@common/types';
import parseNumber from '@common/utils/number/parseNumber';
import Yup from '@common/validation/yup';
import merge from 'lodash/merge';
import pick from 'lodash/pick';
import React, { useCallback, useMemo, useState } from 'react';
import { ObjectSchema, Schema } from 'yup';

type FormValues = Partial<OmitStrict<AxisConfiguration, 'dataSets' | 'type'>>;

interface Props {
  canSaveChart: boolean;
  id: string;
  defaultDataType?: AxisGroupBy;
  type: AxisType;
  configuration: AxisConfiguration;
  data: TableDataResult[];
  meta: FullTableMeta;
  capabilities: ChartCapabilities;
  onChange: (configuration: AxisConfiguration) => void;
  onFormStateChange: (state: { form: AxisType; isValid: boolean }) => void;
  onSubmit: (configuration: AxisConfiguration) => void;
}

const ChartAxisConfiguration = ({
  canSaveChart,
  id,
  configuration,
  data,
  meta,
  type,
  capabilities,
  onChange,
  onFormStateChange,
  onSubmit,
}: Props) => {
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
    type,
  ]);

  const initialValues = useMemo<FormValues>(
    () => pick(configuration, Object.keys(validationSchema.fields)),
    [configuration, validationSchema.fields],
  );

  return (
    <Formik<FormValues>
      initialValues={initialValues}
      enableReinitialize
      validationSchema={validationSchema}
      isInitialValid={validationSchema.isValidSync(initialValues)}
      onSubmit={values => {
        onSubmit(normalizeValues(values));
      }}
      render={form => (
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
            }}
            onMount={onFormStateChange}
            onChange={onFormStateChange}
          />

          <FormGroup>
            {validationSchema.fields.groupBy && (
              <FormFieldSelect<AxisConfiguration>
                id={`${id}-groupBy`}
                label="Group data by"
                name="groupBy"
                options={groupByOptions}
              />
            )}

            {validationSchema.fields.size && (
              <FormFieldNumberInput<AxisConfiguration>
                id={`${id}-size`}
                name="size"
                min="0"
                max="100"
                label="Size of axis (px)"
                width={3}
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
                label="Show axis labels?"
                conditional={
                  <FormFieldTextInput<AxisConfiguration>
                    id={`${id}-unit`}
                    label="Override displayed unit"
                    name="unit"
                    hint="Leave blank to set default from metadata"
                    width={10}
                  />
                }
              />
            )}
          </FormGroup>

          {type === 'minor' &&
            (validationSchema.fields.min || validationSchema.fields.max) && (
              <FormFieldset
                id={`${id}-minorAxisRange`}
                legend="Axis range"
                legendSize="m"
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

          {validationSchema.fields.tickConfig && (
            <FormFieldRadioGroup<AxisConfiguration>
              id={`${id}-tickConfig`}
              name="tickConfig"
              legend="Tick display type"
              legendSize="m"
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

          {type === 'major' && (
            <>
              {validationSchema.fields.sortAsc && (
                <FormFieldset id={`${id}-sort`} legend="Sorting" legendSize="m">
                  {/* <FormFieldSelect<AxisConfiguration>*/}
                  {/*  id={`${id}-sortBy`}*/}
                  {/*  name="sortBy"*/}
                  {/*  label="Sort data by"*/}
                  {/*  options={sortOptions}*/}
                  {/* />*/}
                  <FormFieldCheckbox<AxisConfiguration>
                    id={`${id}-sortAsc`}
                    name="sortAsc"
                    label="Sort Ascending"
                  />
                </FormFieldset>
              )}

              {(validationSchema.fields.min || validationSchema.fields.max) && (
                <FormFieldset
                  id={`${id}-majorAxisRange`}
                  legend="Limiting data"
                  legendSize="m"
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
            </>
          )}

          {validationSchema.fields.referenceLines && (
            <table className="govuk-table">
              <caption className="govuk-heading-m">Reference lines</caption>
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
                      <FormTextInput
                        name={`referenceLines[${form.values.referenceLines?.length}].position`}
                        id={`${id}-referenceLines-position`}
                        label=""
                        defaultValue={`${referenceLine.position}`}
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
                        label=""
                        value={referenceLine.position}
                        order={[]}
                        placeholder="Select position"
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
                      label=""
                      defaultValue={referenceLine.label}
                      onChange={e => {
                        setReferenceLine({
                          ...referenceLine,
                          label: e.target.value,
                        });
                      }}
                    />
                  </td>
                  <td>
                    <button
                      disabled={
                        referenceLine.position === '' ||
                        referenceLine.label === ''
                      }
                      className="govuk-button govuk-!-margin-bottom-0"
                      type="button"
                      onClick={() => {
                        form.setFieldValue('referenceLines', [
                          ...(form.values.referenceLines || []),
                          referenceLine,
                        ]);

                        setReferenceLine({ label: '', position: '' });
                      }}
                    >
                      Add
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          )}

          {form.isValid && form.submitCount > 0 && !canSaveChart && (
            <ErrorSummary
              title="Cannot save chart"
              errors={[
                {
                  id: `${id}-submit`,
                  message: 'Ensure that all other tabs are valid first',
                },
              ]}
              id={`${id}-errorSummary`}
            />
          )}

          <Button type="submit" id={`${id}-submit`}>
            Save chart options
          </Button>
        </Form>
      )}
    />
  );
};

export default ChartAxisConfiguration;
