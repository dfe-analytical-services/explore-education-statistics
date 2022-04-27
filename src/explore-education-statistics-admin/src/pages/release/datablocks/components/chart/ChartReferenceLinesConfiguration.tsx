import Button from '@common/components/Button';
import { FormTextInput } from '@common/components/form';
import FormNumberInput from '@common/components/form/FormNumberInput';
import {
  AxisConfiguration,
  AxisType,
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
  axisType: AxisType;
  configuration: AxisConfiguration;
  id: string;
  meta: FullTableMeta;
}

export default function ChartReferenceLinesConfiguration({
  axisType,
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
    if (axisType === 'minor') {
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
  }, [
    axisType,
    configuration.groupBy,
    meta.indicators,
    meta.locations,
    meta.timePeriodRange,
  ]);

  const filteredOptions = useMemo<SelectOption[]>(() => {
    return options.filter(option =>
      form.values.referenceLines?.every(line => line.position !== option.value),
    );
  }, [form.values.referenceLines, options]);

  const referenceLines = useMemo(() => {
    if (axisType === 'major') {
      return (
        form.values.referenceLines?.filter(line =>
          options.some(option => option.value === line.position),
        ) ?? []
      );
    }

    return form.values.referenceLines ?? [];
  }, [axisType, form.values.referenceLines, options]);

  const getPositionLabel = (position: string | number) => {
    if (axisType === 'major') {
      return options.find(option => option.value === position)?.label;
    }

    return position;
  };

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
        {referenceLines.map(refLine => (
          <tr key={`${refLine.label}_${refLine.position}`}>
            <td>{getPositionLabel(refLine.position)}</td>
            <td>{refLine.label}</td>
            <td>
              <Button
                className="govuk-!-margin-bottom-0 dfe-float--right"
                variant="secondary"
                onClick={() => {
                  const newReferenceLines = form.values.referenceLines?.filter(
                    line =>
                      line.position !== refLine.position &&
                      line.label !== refLine.label,
                  );

                  form.setFieldValue('referenceLines', newReferenceLines);
                }}
              >
                Remove <span className="govuk-visually-hidden">line</span>
              </Button>
            </td>
          </tr>
        ))}
        {(filteredOptions.length > 0 || axisType === 'minor') && (
          <tr>
            <td>
              {axisType === 'major' ? (
                <FormSelect
                  name={`referenceLines[${form.values.referenceLines?.length}].position`}
                  id={`${id}-referenceLines-position`}
                  label="Position"
                  hideLabel
                  value={referenceLine.position?.toString()}
                  placeholder="Choose position"
                  order={FormSelect.unordered}
                  options={filteredOptions}
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
        )}
      </tbody>
    </table>
  );
}
