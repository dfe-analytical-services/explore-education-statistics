import styles from '@admin/pages/release/edit-release/manage-datablocks/components/graph-builder.module.scss';
import Button from '@common/components/Button';
import Effect from '@common/components/Effect';
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
import React, { useCallback, useMemo, useState } from 'react';
import { Schema } from 'yup';
import omit from 'lodash/omit';

type FormValues = OmitStrict<AxisConfiguration, 'dataSets' | 'type'>;

type AxisConfigurationChangeValue = AxisConfiguration & { isValid: boolean };

interface Props {
  id: string;
  defaultDataType?: AxisGroupBy;
  type: AxisType;
  configuration: AxisConfiguration;
  data: TableDataResult[];
  meta: FullTableMeta;
  capabilities: ChartCapabilities;
  onChange: (configuration: AxisConfigurationChangeValue) => void;
  onSubmit: (configuration: AxisConfiguration) => void;
}

const ChartAxisConfiguration = ({
  id,
  configuration,
  data,
  meta,
  type,
  capabilities,
  onChange,
  onSubmit,
}: Props) => {
  const dataSetCategories = useMemo<DataSetCategory[]>(() => {
    if (configuration.type === 'minor') {
      return [];
    }

    const config: AxisConfiguration = {
      ...configuration,
      min: 0,
      max: undefined,
    };

    return createDataSetCategories(config, data, meta);
  }, [configuration, data, meta]);

  // TODO: Figure out how we should sort data
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
      // Values of min/max may be treated as strings by Formik
      // due to the way they are encoded in the form input value.
      return {
        ...configuration,
        ...values,
        min: parseNumber(values.min),
        max: parseNumber(values.max),
        size: parseNumber(values.size),
        tickSpacing: parseNumber(values.tickSpacing),
      };
    },
    [configuration],
  );

  const handleFormChange = useCallback(
    (values: FormValues & { isValid: boolean }) => {
      onChange({
        ...normalizeValues(values),
        isValid: values.isValid,
      });
    },
    [normalizeValues, onChange],
  );

  return (
    <Formik<FormValues>
      initialValues={omit(configuration, ['dataSets', 'type'])}
      enableReinitialize
      onSubmit={values => {
        onSubmit(normalizeValues(values));
      }}
      validationSchema={Yup.object<FormValues>({
        size: Yup.number()
          .required('Enter size of axis')
          .positive('Size of axis must be positive'),
        sortAsc: Yup.boolean(),
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
        referenceLines: Yup.array(),
      })}
      render={form => (
        <Form id={id}>
          <Effect
            value={{
              ...form.values,
              isValid: form.isValid,
            }}
            onChange={handleFormChange}
          />

          <FormGroup>
            {configuration.type === 'major' && !capabilities.fixedAxisGroupBy && (
              <FormFieldSelect<AxisConfiguration>
                id={`${id}-groupBy`}
                label="Group data by"
                name="groupBy"
                options={[
                  {
                    label: 'Time periods',
                    value: 'timePeriod' as AxisGroupBy,
                  },
                  {
                    label: 'Locations',
                    value: 'locations' as AxisGroupBy,
                  },
                  {
                    label: 'Indicators',
                    value: 'indicators' as AxisGroupBy,
                  },
                  {
                    label: 'Filters',
                    value: 'filters' as AxisGroupBy,
                  },
                ]}
              />
            )}

            {capabilities.hasAxes && (
              <FormFieldTextInput<AxisConfiguration>
                id={`${id}-size`}
                name="size"
                type="number"
                min="0"
                max="100"
                label="Size of axis (px)"
                width={3}
              />
            )}

            {capabilities.gridLines && (
              <FormFieldCheckbox<AxisConfiguration>
                id={`${id}-showGrid`}
                name="showGrid"
                label="Show grid lines"
              />
            )}

            {capabilities.hasAxes && (
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

          {configuration.type === 'minor' && (
            <FormFieldset
              id={`${id}-minorAxisRange`}
              legend="Axis range"
              legendSize="m"
              hint="Leaving these values blank will set them to 'auto'"
            >
              <div className={styles.axisRange}>
                <FormFieldTextInput<AxisConfiguration>
                  id={`${id}-minorMin`}
                  name="min"
                  type="number"
                  width={10}
                  label="Minimum value"
                  formGroupClass="govuk-!-margin-right-2"
                />
                <FormFieldTextInput<AxisConfiguration>
                  id={`${id}-minorMax`}
                  name="max"
                  type="number"
                  width={10}
                  label="Maximum value"
                />
              </div>
            </FormFieldset>
          )}

          {capabilities.hasAxes && (
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
                    <FormFieldTextInput<AxisConfiguration>
                      id={`${id}-tickSpacing`}
                      name="tickSpacing"
                      type="number"
                      width={10}
                      label="Every nth value"
                    />
                  ),
                },
              ]}
            />
          )}

          {configuration.type === 'major' && (
            <>
              {capabilities.canSort && (
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

              <FormFieldset
                id={`${id}-majorAxisRange`}
                legend="Limiting data"
                legendSize="m"
              >
                <FormFieldSelect<AxisConfiguration>
                  id={`${id}-majorMin`}
                  label="Minimum"
                  name="min"
                  placeholder="Default"
                  options={limitOptions}
                />
                <FormFieldSelect<AxisConfiguration>
                  id={`${id}-majorMax`}
                  label="Maximum"
                  name="max"
                  placeholder="Default"
                  options={limitOptions}
                />
              </FormFieldset>
            </>
          )}

          {capabilities.hasReferenceLines && (
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
                    {configuration.type === 'minor' && (
                      <FormTextInput
                        name={`referenceLines[${form.values.referenceLines?.length}].position`}
                        id={`${id}-referenceLines-position`}
                        label=""
                        type="text"
                        defaultValue={`${referenceLine.position}`}
                        onChange={e => {
                          setReferenceLine({
                            ...referenceLine,
                            position: e.target.value,
                          });
                        }}
                      />
                    )}
                    {configuration.type === 'major' && (
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
                      type="text"
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

          <Button type="submit">Save chart options</Button>
        </Form>
      )}
    />
  );
};

export default ChartAxisConfiguration;
