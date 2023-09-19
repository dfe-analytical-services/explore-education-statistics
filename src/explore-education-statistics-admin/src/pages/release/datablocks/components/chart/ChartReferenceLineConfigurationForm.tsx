import styles from '@admin/pages/release/datablocks/components/chart/ChartReferenceLineConfigurationForm.module.scss';
import Button from '@common/components/Button';
import { FormFieldset } from '@common/components/form';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import Tooltip from '@common/components/Tooltip';
import {
  ChartDefinitionAxis,
  ReferenceLine,
  ReferenceLineStyle,
} from '@common/modules/charts/types/chart';
import { MinorAxisDomainValues } from '@common/modules/charts/util/domainTicks';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import { produce } from 'immer';
import isEqual from 'lodash/isEqual';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

const otherAxisPositionTypes = {
  default: 'default',
  betweenDataPoints: 'between-data-points',
  custom: 'custom',
} as const;
type OtherAxisPositionTypeKey = keyof typeof otherAxisPositionTypes;
type OtherAxisPositionType =
  (typeof otherAxisPositionTypes)[OtherAxisPositionTypeKey];

interface FormValues {
  label: string;
  labelWidth?: number;
  position: string | number;
  otherAxisEnd?: string;
  otherAxisPosition?: number;
  otherAxisPositionType?: OtherAxisPositionType;
  otherAxisStart?: string;
  style: ReferenceLineStyle;
}

interface Props {
  allReferenceLines: ReferenceLine[];
  axisDefinition?: ChartDefinitionAxis;
  axisPositionOptions: SelectOption[];
  majorAxisOptions: SelectOption[];
  minorAxisDomain?: MinorAxisDomainValues;
  referenceLine?: ReferenceLine;
  onSubmit: (referenceLines: ReferenceLine[]) => void;
}

export default function ChartReferenceLineConfigurationForm({
  allReferenceLines,
  axisDefinition,
  axisPositionOptions,
  majorAxisOptions,
  minorAxisDomain,
  referenceLine,
  onSubmit,
}: Props) {
  const { axis, referenceLineDefaults, type } = axisDefinition || {};

  const validationSchema: ObjectSchema<FormValues> = useMemo(() => {
    return Yup.object({
      label: Yup.string().required('Enter label'),
      labelWidth: Yup.number().optional(),
      position: Yup.mixed<FormValues['position']>()
        .required('Enter position')
        .test({
          name: 'axisPosition',
          message: `Enter a position within the ${
            axis === 'x' ? 'X' : 'Y'
          } axis min/max range`,
          test: value => {
            if (typeof value !== 'number') {
              return true;
            }

            return type === 'minor' && minorAxisDomain
              ? value >= minorAxisDomain?.min && value <= minorAxisDomain.max
              : true;
          },
        }),
      style: Yup.string()
        .required('Enter style')
        .oneOf<ReferenceLineStyle>(['dashed', 'solid', 'none']),
      otherAxisEnd: Yup.string()
        .optional()
        .when('otherAxisPositionType', {
          is: otherAxisPositionTypes.betweenDataPoints,
          then: s => s.required('Enter end point').min(1, 'Enter end point'),
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
        .when('otherAxisPositionType', {
          is: otherAxisPositionTypes.betweenDataPoints,
          then: s =>
            s.required('Enter start point').min(1, 'Enter start point'),
          otherwise: s => s.notRequired(),
        }),
      otherAxisPosition:
        type === 'minor'
          ? Yup.number().when('otherAxisPositionType', {
              is: otherAxisPositionTypes.custom,
              then: s =>
                s.required('Enter a percentage between 0 and 100%').test({
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
          : Yup.number().test({
              name: 'otherAxisPosition',
              message: `Enter a position within the ${
                axis === 'x' ? 'Y' : 'X'
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
        .oneOf<OtherAxisPositionType>(Object.values(otherAxisPositionTypes)),
    });
  }, [axis, minorAxisDomain, type]);

  const initialValues: FormValues = useMemo(() => {
    return {
      label: referenceLine?.label ?? '',
      labelWidth: referenceLine?.labelWidth ?? undefined,
      otherAxisEnd: referenceLine?.otherAxisEnd ?? undefined,
      otherAxisPosition: referenceLine?.otherAxisPosition ?? undefined,
      otherAxisStart: referenceLine?.otherAxisStart ?? undefined,
      otherAxisPositionType:
        type === 'minor' ? getOtherAxisPositionType(referenceLine) : undefined,
      position: referenceLine?.position ?? '',
      style: referenceLine?.style ?? referenceLineDefaults?.style ?? 'dashed',
    };
  }, [referenceLine, referenceLineDefaults?.style, type]);

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={initialValues}
      validationSchema={validationSchema}
      onSubmit={async (values, helpers) => {
        const updated = produce(values, draft => {
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

        if (
          allReferenceLines
            .filter(line => !isEqual(line, referenceLine))
            .some(line => isEqual(line, updated))
        ) {
          helpers.setErrors({
            position: 'A line with these settings has already been added',
          });
          return;
        }

        const updatedReferenceLines = referenceLine
          ? allReferenceLines?.map(line => {
              return isEqual(line, referenceLine) ? updated : line;
            })
          : [...(allReferenceLines ?? []), updated];

        await onSubmit(updatedReferenceLines);
        helpers.resetForm();
      }}
    >
      {form => (
        <>
          <td className="dfe-vertical-align--bottom">
            {type === 'major' ? (
              <FormFieldSelect<FormValues>
                name="position"
                id="referenceLines-position"
                label="Position"
                formGroup={false}
                hideLabel
                placeholder="Choose position"
                order={FormSelect.unordered}
                options={axisPositionOptions}
              />
            ) : (
              <FormFieldNumberInput<FormValues>
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
                <FormFieldSelect<FormValues>
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

                {form.values.otherAxisPositionType !==
                  otherAxisPositionTypes.default && (
                  <>
                    {form.values.otherAxisPositionType ===
                    otherAxisPositionTypes.custom ? (
                      <div className="govuk-!-margin-top-2">
                        <FormFieldNumberInput<FormValues>
                          name="otherAxisPosition"
                          id="referenceLines-otherAxisPosition"
                          label={`Percent along ${
                            axis === 'x' ? 'Y' : 'X'
                          } axis`}
                          formGroup={false}
                          width={10}
                        />
                      </div>
                    ) : (
                      <FormFieldset
                        className="govuk-!-margin-top-2"
                        id="referenceLines-betweenPosition"
                        legend="Select start and end points of line"
                        legendSize="s"
                        legendWeight="regular"
                      >
                        <FormFieldSelect<FormValues>
                          className="govuk-!-margin-bottom-2"
                          name="otherAxisStart"
                          id="referenceLines-otherAxisStart"
                          label="Start point"
                          formGroup={false}
                          hideLabel
                          placeholder="Start point"
                          order={FormSelect.unordered}
                          options={majorAxisOptions}
                        />
                        <br />
                        <FormFieldSelect<FormValues>
                          name="otherAxisEnd"
                          id="referenceLines-otherAxisEnd"
                          label="End point"
                          formGroup={false}
                          hideLabel
                          placeholder="End point"
                          order={FormSelect.unordered}
                          options={majorAxisOptions}
                        />
                      </FormFieldset>
                    )}
                  </>
                )}
              </>
            ) : (
              <FormFieldNumberInput<FormValues>
                name="otherAxisPosition"
                id="referenceLines-otherAxisPosition"
                label={axis === 'x' ? 'Y axis position' : 'X axis position'}
                formGroup={false}
                hideLabel
                hint="Value (optional)"
                width={10}
              />
            )}
          </td>
          <td className="govuk-!-width-one-third dfe-vertical-align--bottom">
            <FormFieldTextInput<FormValues>
              name="label"
              id="referenceLines-label"
              label="Label"
              formGroup={false}
              hideLabel
            />
          </td>
          <td className="dfe-vertical-align--bottom">
            <FormFieldNumberInput<FormValues>
              name="labelWidth"
              id="referenceLines-labelWidth"
              hint="Pixels (optional)"
              label="Label width"
              formGroup={false}
              hideLabel
              width={10}
            />
          </td>
          <td className="dfe-vertical-align--bottom">
            <FormFieldSelect<FormValues>
              className={styles.styleSelect}
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
              text={!form.isValid ? 'Cannot add invalid reference line' : ''}
              enabled={!form.isValid}
            >
              {({ ref }) => (
                <Button
                  ariaDisabled={!form.isValid}
                  className="govuk-!-margin-bottom-0 dfe-float--right"
                  ref={ref}
                  onClick={async () => {
                    await form.submitForm();
                  }}
                >
                  {referenceLine ? 'Save' : 'Add line'}
                </Button>
              )}
            </Tooltip>
          </td>
        </>
      )}
    </Formik>
  );
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
