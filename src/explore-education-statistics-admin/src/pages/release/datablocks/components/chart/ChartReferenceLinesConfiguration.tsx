import Button from '@common/components/Button';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Tooltip from '@common/components/Tooltip';
import {
  AxisConfiguration,
  AxisType,
} from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import Yup from '@common/validation/yup';
import FormSelect, {
  SelectOption,
} from 'explore-education-statistics-common/src/components/form/FormSelect';
import { Formik, useFormikContext } from 'formik';
import React, { useMemo } from 'react';

interface FormValues {
  referenceLines?: AxisConfiguration['referenceLines'];
}

interface AddFormValues {
  label: string;
  position: string;
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
          <Formik<AddFormValues>
            initialValues={{
              label: '',
              position: '',
            }}
            validationSchema={Yup.object<AddFormValues>({
              label: Yup.string().required('Enter label'),
              position: Yup.string().required('Enter position'),
            })}
            onSubmit={(values, helpers) => {
              form.setFieldValue('referenceLines', [
                ...(form.values.referenceLines ?? []),
                values,
              ]);

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
