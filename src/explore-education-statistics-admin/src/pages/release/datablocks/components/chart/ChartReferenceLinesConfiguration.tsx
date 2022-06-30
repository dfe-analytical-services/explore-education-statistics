import Button from '@common/components/Button';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Tooltip from '@common/components/Tooltip';
import {
  AxisType,
  ChartDefinition,
  ReferenceLine,
  ReferenceLineStyle,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import Yup from '@common/validation/yup';
import FormSelect, {
  SelectOption,
} from 'explore-education-statistics-common/src/components/form/FormSelect';
import { Formik } from 'formik';
import upperFirst from 'lodash/upperFirst';
import React, { useMemo } from 'react';

interface AddFormValues {
  label: string;
  position: string | number;
  style: ReferenceLineStyle;
}

export interface ChartReferenceLinesConfigurationProps {
  axisType: AxisType;
  dataSetCategories: DataSetCategory[];
  definition: ChartDefinition;
  id: string;
  lines: ReferenceLine[];
  onAddLine: (line: ReferenceLine) => void;
  onRemoveLine: (line: ReferenceLine) => void;
}

export default function ChartReferenceLinesConfiguration({
  axisType,
  dataSetCategories,
  definition,
  id,
  lines,
  onAddLine,
  onRemoveLine,
}: ChartReferenceLinesConfigurationProps) {
  const options = useMemo<SelectOption[]>(() => {
    if (axisType === 'minor') {
      return [];
    }

    return dataSetCategories.map(({ filter }) => ({
      label: filter.label,
      value: filter.value,
    }));
  }, [axisType, dataSetCategories]);

  const filteredOptions = useMemo<SelectOption[]>(() => {
    return options.filter(option =>
      lines?.every(line => line.position !== option.value),
    );
  }, [lines, options]);

  const referenceLines = useMemo(() => {
    if (axisType === 'major') {
      return (
        lines?.filter(line =>
          options.some(option => option.value === line.position),
        ) ?? []
      );
    }

    return lines ?? [];
  }, [axisType, lines, options]);

  const getPositionLabel = (position: string | number) => {
    if (axisType === 'major') {
      return options.find(option => option.value === position)?.label;
    }

    return position;
  };

  const axisDefinition = definition.axes[axisType];

  return (
    <table className="govuk-table">
      <caption className="govuk-heading-s">Reference lines</caption>
      <thead>
        <tr>
          <th>Position</th>
          <th>Label</th>
          <th>Style</th>
          <th className="dfe-align--right">Actions</th>
        </tr>
      </thead>
      <tbody>
        {referenceLines.map(line => (
          <tr key={`${line.label}_${line.position}`}>
            <td>{getPositionLabel(line.position)}</td>
            <td>{line.label}</td>
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
        {(filteredOptions.length > 0 || axisType === 'minor') && (
          <Formik<AddFormValues>
            initialValues={{
              label: '',
              position: '',
              style: 'dashed',
              ...(axisDefinition?.referenceLineDefaults ?? {}),
            }}
            validationSchema={Yup.object<AddFormValues>({
              label: Yup.string().required('Enter label'),
              position: Yup.string().required('Enter position'),
              style: Yup.string()
                .required('Enter style')
                .oneOf<ReferenceLineStyle>(['dashed', 'solid', 'none']),
            })}
            onSubmit={(values, helpers) => {
              onAddLine(values);
              helpers.resetForm();
            }}
          >
            {addForm => (
              <tr>
                <td className="dfe-vertical-align--bottom">
                  {axisType === 'major' ? (
                    <FormFieldSelect
                      name="position"
                      id={`${id}-referenceLines-position`}
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
                      id={`${id}-referenceLines-position`}
                      label="Position"
                      formGroup={false}
                      hideLabel
                    />
                  )}
                </td>
                <td className="dfe-vertical-align--bottom">
                  <FormFieldTextInput
                    name="label"
                    id={`${id}-referenceLines-label`}
                    label="Label"
                    formGroup={false}
                    hideLabel
                  />
                </td>
                <td className="dfe-vertical-align--bottom">
                  <FormFieldSelect
                    name="style"
                    id={`${id}-referenceLines-style`}
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
