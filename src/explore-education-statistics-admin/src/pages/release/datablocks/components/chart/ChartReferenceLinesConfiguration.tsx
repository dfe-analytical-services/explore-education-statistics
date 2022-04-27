import Button from '@common/components/Button';
import { FormTextInput } from '@common/components/form';
import FormNumberInput from '@common/components/form/FormNumberInput';
import {
  AxisConfiguration,
  ReferenceLine,
} from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import FormSelect, {
  SelectOption,
} from 'explore-education-statistics-common/src/components/form/FormSelect';
import { useFormikContext } from 'formik';
import React, { useMemo, useState } from 'react';

interface FormValues {
  referenceLines?: AxisConfiguration['referenceLines'];
}

interface Props {
  configuration: AxisConfiguration;
  id: string;
  meta: FullTableMeta;
}

export default function ChartReferenceLinesConfiguration({
  configuration,
  id,
  meta,
}: Props) {
  const form = useFormikContext<FormValues>();

  const [referenceLine, setReferenceLine] = useState<ReferenceLine>({
    position: '',
    label: '',
  });

  const options = useMemo<SelectOption[]>(() => {
    if (!configuration.groupBy) {
      return [];
    }

    switch (configuration.groupBy) {
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
  }, [configuration.groupBy, meta]);

  return (
    <table className="govuk-table">
      <caption className="govuk-heading-s">Reference lines</caption>
      <thead>
        <tr>
          <th>Position</th>
          <th>Label</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {form.values.referenceLines?.map((refLine, index) => (
          <tr key={`${refLine.label}_${refLine.position}`}>
            <td>{refLine.position}</td>
            <td>{refLine.label}</td>
            <td>
              <Button
                className="govuk-!-margin-bottom-0 dfe-float--right"
                variant="secondary"
                onClick={() => {
                  const newReferenceLines = [
                    ...(form.values.referenceLines ?? []),
                  ];
                  newReferenceLines.splice(index, 1);

                  form.setFieldValue('referenceLines', newReferenceLines);
                }}
              >
                Remove
              </Button>
            </td>
          </tr>
        ))}
        <tr>
          <td>
            {configuration.groupBy ? (
              <FormSelect
                name={`referenceLines[${form.values.referenceLines?.length}].position`}
                id={`${id}-referenceLines-position`}
                label="Position"
                hideLabel
                value={referenceLine.position?.toString()}
                placeholder="Select position"
                order={FormSelect.unordered}
                options={options}
                onChange={e => {
                  setReferenceLine({
                    ...referenceLine,
                    position: e.target.value,
                  });
                }}
              />
            ) : (
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
              className="govuk-!-margin-bottom-0 dfe-float--right"
              disabled={!referenceLine.position || !referenceLine.label}
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
  );
}
