import styles from '@admin/pages/release/datablocks/components/chart/ChartReferenceLinesConfiguration.module.scss';
import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import Tooltip from '@common/components/Tooltip';
import VisuallyHidden from '@common/components/VisuallyHidden';
import {
  AxisType,
  ChartDefinitionAxis,
  ReferenceLine,
  ReferenceLineStyle,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import { MinorAxisDomainValues } from '@common/modules/charts/util/domainTicks';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import produce from 'immer';
import upperFirst from 'lodash/upperFirst';
import React, { useMemo } from 'react';

const otherAxisPositionTypes = {
  default: 'default',
  betweenDataPoints: 'between-data-points',
  custom: 'custom',
} as const;
type OtherAxisPositionType = keyof typeof otherAxisPositionTypes;

interface AddFormValues {
  otherAxisEnd?: string;
  label: string;
  otherAxisPosition?: number;
  otherAxisPositionType?: typeof otherAxisPositionTypes[OtherAxisPositionType];
  position: string | number;
  otherAxisStart?: string;
  style: ReferenceLineStyle;
}

export interface ChartReferenceLinesConfigurationProps {
  axisDefinition?: ChartDefinitionAxis;
  dataSetCategories: DataSetCategory[];
  lines: ReferenceLine[];
  minorAxisDomain?: MinorAxisDomainValues;
  onAddLine: (line: ReferenceLine) => void;
  onRemoveLine: (line: ReferenceLine) => void;
}

export default function ChartReferenceLinesConfiguration({
  axisDefinition,
  dataSetCategories,
  lines,
  minorAxisDomain,
  onAddLine,
  onRemoveLine,
}: ChartReferenceLinesConfigurationProps) {
  const { axis, referenceLineDefaults, type } = axisDefinition || {};

  const majorAxisOptions = useMemo<SelectOption[]>(() => {
    return dataSetCategories.map(({ filter }) => ({
      label: filter.label,
      value:
        filter instanceof LocationFilter
          ? LocationFilter.createId(filter)
          : filter.value,
    }));
  }, [dataSetCategories]);

  const filteredOptions = useMemo<SelectOption[]>(() => {
    return majorAxisOptions.filter(option =>
      lines?.every(line => line.position !== option.value),
    );
  }, [lines, majorAxisOptions]);

  const referenceLines = useMemo(() => {
    if (type === 'major') {
      return (
        lines?.filter(line =>
          majorAxisOptions.some(option => option.value === line.position),
        ) ?? []
      );
    }

    return lines ?? [];
  }, [type, lines, majorAxisOptions]);

  const validationSchema = useMemo(() => {
    const schema = Yup.object<AddFormValues>({
      label: Yup.string().required('Enter label'),
      position: Yup.string()
        .required('Enter position')
        .test({
          name: 'axisPosition',
          message: `Enter a position within the ${
            axis === 'x' ? 'X' : 'Y'
          } axis min/max range`,
          test: (value: number) => {
            return type === 'minor' && minorAxisDomain
              ? value >= minorAxisDomain?.min && value <= minorAxisDomain.max
              : true;
          },
        }),
      style: Yup.string()
        .required('Enter style')
        .oneOf<ReferenceLineStyle>(['dashed', 'solid', 'none']),
    });

    if (type === 'minor') {
      return schema.shape({
        otherAxisEnd: Yup.string()
          .when('otherAxisPositionType', {
            is: otherAxisPositionTypes.betweenDataPoints,
            then: Yup.string().required('Enter end point'),
            otherwise: Yup.string(),
          })
          .test(
            'otherAxisEnd',
            'End point cannot match start point',
            function checkotherAxisEnd(value: number) {
              /* eslint-disable react/no-this-in-sfc */
              if (this.parent.otherAxisStart) {
                return value !== this.parent.otherAxisStart;
              }
              return true;
              /* eslint-enable react/no-this-in-sfc */
            },
          ),
        otherAxisStart: Yup.string().when('otherAxisPositionType', {
          is: otherAxisPositionTypes.betweenDataPoints,
          then: Yup.string().required('Enter start point'),
          otherwise: Yup.string(),
        }),
        otherAxisPosition: Yup.number().when('otherAxisPositionType', {
          is: otherAxisPositionTypes.custom,
          then: Yup.number()
            .required('Enter a percentage between 0 and 100%')
            .test({
              name: 'otherAxisPosition',
              message: 'Enter a percentage between 0 and 100%',
              test: (value: number) => {
                if (typeof value !== 'number') {
                  return true;
                }
                return value >= 0 && value <= 100;
              },
            }),
          otherwise: Yup.number(),
        }),
      });
    }

    return schema.shape({
      otherAxisPosition: Yup.number().test({
        name: 'otherAxisPosition',
        message: `Enter a position within the ${
          axis === 'x' ? 'Y' : 'X'
        } axis min/max range`,
        test: (value: number) => {
          if (typeof value !== 'number') {
            return true;
          }

          return minorAxisDomain
            ? value >= minorAxisDomain?.min && value <= minorAxisDomain.max
            : true;
        },
      }),
    });
  }, [axis, minorAxisDomain, type]);

  return (
    <table className="govuk-table">
      <caption className="govuk-heading-s">Reference lines</caption>
      <thead>
        <tr>
          <th>Position</th>
          <th className="govuk-!-width-one-quarter">
            {axis === 'x' ? 'Y axis position' : 'X axis position'}
            {type === 'minor' && (
              <>
                <br />
                <span className="govuk-hint govuk-!-font-size-16 govuk-!-margin-bottom-0">
                  0% = start of axis
                </span>
              </>
            )}
          </th>
          <th className="govuk-!-width-one-third">Label</th>
          <th>Style</th>
          <th className={`dfe-align--right ${styles.actions}`}>Actions</th>
        </tr>
      </thead>
      <tbody>
        {referenceLines.map(line => (
          <tr key={`${line.label}_${line.position}`}>
            <td>
              {type === 'major'
                ? majorAxisOptions.find(
                    option => option.value === line.position,
                  )?.label
                : line.position}
            </td>
            <td>
              {getOtherAxisPositionLabel({
                majorAxisOptions,
                otherAxisEnd: line.otherAxisEnd,
                otherAxisStart: line.otherAxisStart,
                otherAxisPosition: line.otherAxisPosition,
                type,
              })}
            </td>
            <td className="govuk-!-width-one-third">{line.label}</td>
            <td>{upperFirst(line.style)}</td>
            <td>
              <Button
                className="govuk-!-margin-bottom-0 dfe-float--right"
                variant="secondary"
                onClick={() => {
                  onRemoveLine(line);
                }}
              >
                Remove <span className="govuk-visually-hidden">line</span>
              </Button>
            </td>
          </tr>
        ))}
        {(filteredOptions.length > 0 || type === 'minor') && (
          <Formik<AddFormValues>
            initialValues={{
              label: '',
              otherAxisPositionType:
                type === 'minor' ? otherAxisPositionTypes.default : undefined,
              position: '',
              style: 'dashed',
              ...(referenceLineDefaults ?? {}),
            }}
            validationSchema={validationSchema}
            onSubmit={(values, helpers) => {
              const updatedValues = produce(values, draft => {
                switch (draft.otherAxisPositionType) {
                  case otherAxisPositionTypes.custom:
                    delete draft.otherAxisStart;
                    delete draft.otherAxisEnd;
                    break;

                  case otherAxisPositionTypes.betweenDataPoints:
                    delete draft.otherAxisPosition;
                    break;

                  case otherAxisPositionTypes.default:
                    delete draft.otherAxisEnd;
                    delete draft.otherAxisPosition;
                    delete draft.otherAxisStart;
                    break;
                  default:
                    break;
                }

                delete draft.otherAxisPositionType;
              });
              onAddLine(updatedValues);
              helpers.resetForm();
            }}
          >
            {addForm => (
              <tr>
                <td className="dfe-vertical-align--bottom">
                  {type === 'major' ? (
                    <FormFieldSelect
                      name="position"
                      id="referenceLines-position"
                      label="Position"
                      formGroup={false}
                      hideLabel
                      placeholder="Choose position"
                      order={FormSelect.unordered}
                      options={filteredOptions}
                    />
                  ) : (
                    <FormFieldNumberInput
                      name="position"
                      id="referenceLines-position"
                      label="Position"
                      formGroup={false}
                      hideLabel
                    />
                  )}
                </td>
                <td className="dfe-vertical-align--bottom">
                  {type === 'minor' ? (
                    <>
                      <FormFieldSelect
                        name="otherAxisPositionType"
                        id="referenceLines-otherAxisPositionType"
                        label={`${axis === 'x' ? 'Y' : 'X'} axis position`}
                        formGroup={false}
                        hideLabel
                        order={FormSelect.unordered}
                        options={[
                          {
                            label: 'Default',
                            value: otherAxisPositionTypes.default,
                          },
                          {
                            label: 'Custom',
                            value: otherAxisPositionTypes.custom,
                          },
                          {
                            label: 'Between data points',
                            value: otherAxisPositionTypes.betweenDataPoints,
                          },
                        ]}
                      />

                      {addForm.values.otherAxisPositionType !==
                        otherAxisPositionTypes.default && (
                        <>
                          {addForm.values.otherAxisPositionType ===
                          otherAxisPositionTypes.custom ? (
                            <div className="govuk-!-margin-top-2">
                              <FormFieldNumberInput
                                name="otherAxisPosition"
                                id="referenceLines-otherAxisPosition"
                                label={`Percent along ${
                                  axis === 'x' ? 'Y' : 'X'
                                } axis`}
                                formGroup={false}
                              />
                            </div>
                          ) : (
                            <FormFieldset
                              className="dfe-flex dfe-align-items-end govuk-!-margin-top-2"
                              id="referenceLines-betweenPosition"
                              legend="Select start and end points of line"
                              legendSize="s"
                              legendWeight="regular"
                            >
                              <div className="govuk-!-margin-right-2">
                                <FormFieldSelect
                                  name="otherAxisStart"
                                  id="referenceLines-otherAxisStart"
                                  label="Start point"
                                  formGroup={false}
                                  hideLabel
                                  placeholder="Start point"
                                  order={FormSelect.unordered}
                                  options={majorAxisOptions}
                                />
                              </div>
                              <div>
                                <FormFieldSelect
                                  name="otherAxisEnd"
                                  id="referenceLines-otherAxisEnd"
                                  label="End point"
                                  formGroup={false}
                                  hideLabel
                                  placeholder="End point"
                                  order={FormSelect.unordered}
                                  options={majorAxisOptions}
                                />
                              </div>
                            </FormFieldset>
                          )}
                        </>
                      )}
                    </>
                  ) : (
                    <FormFieldNumberInput
                      name="otherAxisPosition"
                      id="referenceLines-otherAxisPosition"
                      label={
                        axis === 'x' ? 'Y axis position' : 'X axis position'
                      }
                      formGroup={false}
                      hideLabel
                      hint="Value (leave blank for default)"
                    />
                  )}
                </td>
                <td className="govuk-!-width-one-third dfe-vertical-align--bottom">
                  <FormFieldTextInput
                    name="label"
                    id="referenceLines-label"
                    label="Label"
                    formGroup={false}
                    hideLabel
                  />
                </td>
                <td className="dfe-vertical-align--bottom">
                  <FormFieldSelect
                    name="style"
                    id="referenceLines-style"
                    label="Style"
                    formGroup={false}
                    hideLabel
                    order={FormSelect.unordered}
                    options={[
                      { label: 'Dashed', value: 'dashed' },
                      { label: 'Solid', value: 'solid' },
                      { label: 'None', value: 'none' },
                    ]}
                  />
                </td>
                <td className="dfe-vertical-align--bottom">
                  <Tooltip
                    text={
                      !addForm.isValid
                        ? 'Cannot add invalid reference line'
                        : ''
                    }
                    enabled={!addForm.isValid}
                  >
                    {({ ref }) => (
                      <Button
                        ariaDisabled={!addForm.isValid}
                        className="govuk-!-margin-bottom-0 dfe-float--right"
                        ref={ref}
                        onClick={async () => {
                          await addForm.submitForm();
                        }}
                      >
                        Add line
                      </Button>
                    )}
                  </Tooltip>
                </td>
              </tr>
            )}
          </Formik>
        )}
      </tbody>
    </table>
  );
}

function getOtherAxisPositionLabel({
  majorAxisOptions,
  otherAxisEnd,
  otherAxisStart,
  otherAxisPosition,
  type,
}: {
  majorAxisOptions: SelectOption[];
  otherAxisEnd?: string;
  otherAxisStart?: string;
  otherAxisPosition?: number;
  type?: AxisType;
}) {
  if (otherAxisPosition) {
    return `${otherAxisPosition}${type === 'minor' ? '%' : ''}`;
  }

  if (otherAxisEnd && otherAxisStart) {
    return `${
      majorAxisOptions.find(option => option.value === otherAxisStart)?.label
    } - ${
      majorAxisOptions.find(option => option.value === otherAxisEnd)?.label
    }`;
  }

  return null;
}
