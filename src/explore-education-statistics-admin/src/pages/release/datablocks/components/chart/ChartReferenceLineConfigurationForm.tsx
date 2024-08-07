import styles from '@admin/pages/release/datablocks/components/chart/ChartReferenceLineConfigurationForm.module.scss';
import { ChartAxisConfigurationFormValues } from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import Tooltip from '@common/components/Tooltip';
import { FormSelect, FormFieldset } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import {
  ChartDefinitionAxis,
  ChartType,
  ReferenceLine,
} from '@common/modules/charts/types/chart';
import { otherAxisPositionTypes } from '@common/modules/charts/types/referenceLinePosition';
import React from 'react';
import { useFormContext } from 'react-hook-form';
import { produce } from 'immer';

interface Props {
  axisDefinition?: ChartDefinitionAxis;
  axisPositionOptions: SelectOption[];
  chartType: ChartType;
  index: number;
  isEditing?: boolean;
  majorAxisOptions: SelectOption[];
  onSave: (updatedLines: ReferenceLine[]) => void;
}

export default function ChartReferenceLineConfigurationForm({
  axisDefinition,
  axisPositionOptions,
  chartType,
  index,
  isEditing,
  majorAxisOptions,
  onSave,
}: Props) {
  const { formState, setError, watch, trigger } =
    useFormContext<ChartAxisConfigurationFormValues>();
  const values = watch();
  const referenceLine = values.referenceLines?.[index];
  const { axis, type: axisType } = axisDefinition || {};
  return (
    <>
      <td className="dfe-vertical-align--bottom">
        {axisType === 'major' ? (
          <>
            <FormFieldSelect<ChartAxisConfigurationFormValues>
              name={`referenceLines.${index}.position`}
              id="referenceLines-position"
              label="Position"
              formGroup={false}
              hideLabel
              placeholder="Choose position"
              order={FormSelect.unordered}
              options={[
                ...axisPositionOptions,
                ...(chartType === 'verticalbar'
                  ? [
                      {
                        label: 'Between data points',
                        value: otherAxisPositionTypes.betweenDataPoints,
                      },
                    ]
                  : []),
              ]}
              onBlur={() => trigger(`referenceLines.${index}.position`)}
            />
            {referenceLine?.position ===
              otherAxisPositionTypes.betweenDataPoints && (
              <FormFieldset
                className="govuk-!-margin-top-2"
                id="referenceLines-betweenPosition"
                legend="Select start and end points"
                legendSize="s"
                legendWeight="regular"
              >
                <FormFieldSelect<ChartAxisConfigurationFormValues>
                  className="govuk-!-margin-bottom-2"
                  name={`referenceLines.${index}.otherAxisStart`}
                  id="referenceLines-otherAxisStart"
                  label="Start point"
                  formGroup={false}
                  hideLabel
                  placeholder="Start point"
                  order={FormSelect.unordered}
                  options={majorAxisOptions}
                  onBlur={() =>
                    trigger(`referenceLines.${index}.otherAxisStart`)
                  }
                />
                <br />
                <FormFieldSelect<ChartAxisConfigurationFormValues>
                  name={`referenceLines.${index}.otherAxisEnd`}
                  id="referenceLines-otherAxisEnd"
                  label="End point"
                  formGroup={false}
                  hideLabel
                  placeholder="End point"
                  order={FormSelect.unordered}
                  options={majorAxisOptions}
                  onBlur={() => trigger(`referenceLines.${index}.otherAxisEnd`)}
                />
              </FormFieldset>
            )}
          </>
        ) : (
          <FormFieldNumberInput<ChartAxisConfigurationFormValues>
            name={`referenceLines.${index}.position`}
            id="referenceLines-position"
            label="Position"
            formGroup={false}
            hideLabel
            onBlur={() => trigger(`referenceLines.${index}.position`)}
          />
        )}
      </td>
      <td className="dfe-vertical-align--bottom">
        {axisType === 'minor' ? (
          <>
            <FormFieldSelect<ChartAxisConfigurationFormValues>
              name={`referenceLines.${index}.otherAxisPositionType`}
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
              onBlur={() =>
                trigger(`referenceLines.${index}.otherAxisPositionType`)
              }
            />

            {referenceLine?.otherAxisPositionType !==
              otherAxisPositionTypes.default && (
              <>
                {referenceLine?.otherAxisPositionType ===
                otherAxisPositionTypes.custom ? (
                  <div className="govuk-!-margin-top-2">
                    <FormFieldNumberInput<ChartAxisConfigurationFormValues>
                      name={`referenceLines.${index}.otherAxisPosition`}
                      id="referenceLines-otherAxisPosition"
                      label={`Percent along ${axis === 'x' ? 'Y' : 'X'} axis`}
                      formGroup={false}
                      width={10}
                      onBlur={() =>
                        trigger(`referenceLines.${index}.otherAxisPosition`)
                      }
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
                    <FormFieldSelect<ChartAxisConfigurationFormValues>
                      className="govuk-!-margin-bottom-2"
                      name={`referenceLines.${index}.otherAxisStart`}
                      id="referenceLines-otherAxisStart"
                      label="Start point"
                      formGroup={false}
                      hideLabel
                      placeholder="Start point"
                      order={FormSelect.unordered}
                      options={majorAxisOptions}
                      onBlur={() =>
                        trigger(`referenceLines.${index}.otherAxisStart`)
                      }
                    />
                    <br />
                    <FormFieldSelect<ChartAxisConfigurationFormValues>
                      name={`referenceLines.${index}.otherAxisEnd`}
                      id="referenceLines-otherAxisEnd"
                      label="End point"
                      formGroup={false}
                      hideLabel
                      placeholder="End point"
                      order={FormSelect.unordered}
                      options={majorAxisOptions}
                      onBlur={() =>
                        trigger(`referenceLines.${index}.otherAxisEnd`)
                      }
                    />
                  </FormFieldset>
                )}
              </>
            )}
          </>
        ) : (
          <FormFieldNumberInput<ChartAxisConfigurationFormValues>
            name={`referenceLines.${index}.otherAxisPosition`}
            id="referenceLines-otherAxisPosition"
            label={axis === 'x' ? 'Y axis position' : 'X axis position'}
            formGroup={false}
            hideLabel
            hint={`Value ${
              values.referenceLines?.[index].position !==
              otherAxisPositionTypes.betweenDataPoints
                ? '(optional)'
                : ''
            }`}
            width={10}
            onBlur={() => trigger(`referenceLines.${index}.otherAxisPosition`)}
          />
        )}
      </td>
      <td className="govuk-!-width-one-third dfe-vertical-align--bottom">
        <FormFieldTextInput<ChartAxisConfigurationFormValues>
          name={`referenceLines.${index}.label`}
          id="referenceLines-label"
          label="Label"
          formGroup={false}
          hideLabel
          onBlur={() => trigger(`referenceLines.${index}.label`)}
        />
      </td>
      <td className="dfe-vertical-align--bottom">
        <FormFieldNumberInput<ChartAxisConfigurationFormValues>
          name={`referenceLines.${index}.labelWidth`}
          id="referenceLines-labelWidth"
          hint="Pixels (optional)"
          label="Label width"
          formGroup={false}
          hideLabel
          width={10}
          onBlur={() => trigger(`referenceLines.${index}.labelWidth`)}
        />
      </td>
      <td className="dfe-vertical-align--bottom">
        <FormFieldSelect<ChartAxisConfigurationFormValues>
          className={styles.styleSelect}
          name={`referenceLines.${index}.style`}
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
          onBlur={() => trigger(`referenceLines.${index}.style`)}
        />
      </td>
      <td className="dfe-vertical-align--bottom">
        <Tooltip
          text={!formState.isValid ? 'Cannot add invalid reference line' : ''}
          enabled={!formState.isValid}
        >
          {({ ref }) => (
            <ButtonGroup className="govuk-!-margin-0">
              <Button
                ariaDisabled={!formState.isValid}
                className="govuk-!-margin-bottom-0 dfe-float--right"
                ref={ref}
                type="button"
                onClick={() => {
                  if (
                    !referenceLine ||
                    !values.referenceLines?.[index].label ||
                    !values.referenceLines?.[index].position
                  ) {
                    return;
                  }

                  const updatedLine = produce(referenceLine, draft => {
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
                    values.referenceLines.filter(
                      line =>
                        line.position.toString() ===
                          updatedLine.position.toString() &&
                        line.label === updatedLine.label,
                    ).length > 1
                  ) {
                    setError(`referenceLines.${index}.position`, {
                      message:
                        'A line with these settings has already been added',
                    });
                    return;
                  }

                  const updatedLines = values.referenceLines;
                  updatedLines[index] = updatedLine;
                  onSave(updatedLines);
                }}
              >
                {isEditing ? 'Save' : 'Add'}
              </Button>
              {!isEditing && (
                <ButtonText
                  onClick={() => {
                    (values.referenceLines ?? []).splice(index, 1);
                    onSave(values.referenceLines ?? []);
                  }}
                >
                  Remove
                </ButtonText>
              )}
            </ButtonGroup>
          )}
        </Tooltip>
      </td>
    </>
  );
}
