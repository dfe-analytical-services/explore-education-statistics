import { ChartDataGroupingFormValues } from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm';
import Button from '@common/components/Button';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import Tooltip from '@common/components/Tooltip';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { maxMapDataGroups } from '@common/modules/charts/components/MapBlock';
import { CustomDataGroup } from '@common/modules/charts/types/chart';
import React from 'react';
import { useFormContext } from 'react-hook-form';

interface Props {
  groups?: CustomDataGroup[];
  id: string;
  unit?: string;
  onAddGroup: () => void;
  onRemoveGroup: (group: CustomDataGroup) => void;
}

export default function ChartCustomDataGroupingsConfiguration({
  groups = [],
  id,
  unit,
  onAddGroup,
  onRemoveGroup,
}: Props) {
  const unitLabel = unit ? ` (${unit})` : '';

  const {
    formState: { errors, touchedFields },
    getValues,
  } = useFormContext<ChartDataGroupingFormValues>();

  // Take into account that values can be zero.
  const hasMinValue = getValues('min') !== undefined;
  const hasMaxValue = getValues('max') !== undefined;

  const canSubmit = !errors.min && !errors.max && hasMinValue && hasMaxValue;

  return (
    <table id={id}>
      <caption>
        <VisuallyHidden>Custom groups</VisuallyHidden>
      </caption>
      <thead>
        <tr>
          <th>{`Min ${unitLabel}`}</th>
          <th>{`Max ${unitLabel}`}</th>
          <th className="govuk-!-text-align-right">Actions</th>
        </tr>
      </thead>
      <tbody>
        {groups.map((group, index) => (
          <tr key={`group-${index.toString()}`}>
            <td>{group.min}</td>
            <td>{group.max}</td>
            <td>
              <Button
                className="govuk-!-margin-bottom-0 dfe-float--right"
                variant="secondary"
                onClick={() => {
                  onRemoveGroup(group);
                }}
              >
                Remove group
              </Button>
            </td>
          </tr>
        ))}
        {groups.length < maxMapDataGroups && (
          <tr>
            <td className="dfe-vertical-align--bottom">
              <FormFieldNumberInput
                errorString={
                  touchedFields.min && !hasMinValue
                    ? 'Enter a minimum value'
                    : ''
                }
                name="min"
                formGroup={false}
                hideLabel
                label={`Min${unitLabel}`}
                width={10}
              />
            </td>
            <td className="dfe-vertical-align--bottom">
              <FormFieldNumberInput
                errorString={
                  touchedFields.max && !hasMaxValue
                    ? 'Enter a maximum value'
                    : ''
                }
                name="max"
                formGroup={false}
                hideLabel
                label={`Max${unitLabel}`}
                width={10}
              />
            </td>
            <td className="dfe-vertical-align--bottom">
              <Tooltip
                text={!canSubmit ? 'Cannot add invalid group' : ''}
                enabled={!canSubmit}
              >
                {({ ref }) => (
                  <Button
                    ariaDisabled={!canSubmit}
                    className="govuk-!-margin-bottom-0 dfe-float--right"
                    ref={ref}
                    onClick={onAddGroup}
                  >
                    Add group
                  </Button>
                )}
              </Tooltip>
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
}
