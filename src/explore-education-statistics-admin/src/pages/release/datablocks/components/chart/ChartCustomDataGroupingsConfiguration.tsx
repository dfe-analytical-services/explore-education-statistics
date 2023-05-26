import Button from '@common/components/Button';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import Tooltip from '@common/components/Tooltip';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { CustomDataGroup } from '@common/modules/charts/types/chart';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';

interface FormValues {
  min?: number;
  max?: number;
}

interface Props {
  groups?: CustomDataGroup[];
  id: string;
  unit?: string;
  onAddGroup: (group: CustomDataGroup) => void;
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

  return (
    <table id={id}>
      <caption>
        <VisuallyHidden>Custom groups</VisuallyHidden>
      </caption>
      <thead>
        <tr>
          <th>{`Min ${unitLabel}`}</th>
          <th>{`Max ${unitLabel}`}</th>
          <th className="dfe-align--right">Actions</th>
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
        <Formik<FormValues>
          initialValues={{ max: undefined, min: undefined }}
          validationSchema={Yup.object<FormValues>({
            min: Yup.number()
              .required('Enter a minimum value')
              .test(
                'noOverlap',
                'Min cannot overlap another group',
                function noOverlap(value: number) {
                  // Show error when the min is in an existing group
                  if (
                    groups.some(
                      group => value >= group.min && value <= group.max,
                    )
                  ) {
                    return false;
                  }

                  /* eslint-disable react/no-this-in-sfc */
                  // This group overlaps with an existing group
                  const overlaps = groups.some(
                    group => value <= group.max && group.min <= this.parent.max,
                  );

                  if (overlaps) {
                    // Check if the max is in an existing group,
                    // don't show the error here if it is as only want
                    // to show the error on the field(s) that require action.
                    return groups.some(
                      group =>
                        this.parent.max >= group.min &&
                        this.parent.max <= group.max,
                    );
                  }
                  /* eslint-enable react/no-this-in-sfc */
                  return true;
                },
              ),
            max: Yup.number()
              .required('Enter a maximum value')
              .moreThan(Yup.ref('min'), 'Must be greater than min')
              .test(
                'noOverlap',
                'Max cannot overlap another group',
                function noOverlap(value: number) {
                  // Show error when the max is in an existing group
                  if (
                    groups.some(
                      group => value >= group.min && value <= group.max,
                    )
                  ) {
                    return false;
                  }

                  /* eslint-disable react/no-this-in-sfc */
                  // This group overlaps with an existing group
                  const overlaps = groups.some(
                    group => this.parent.min <= group.max && group.min <= value,
                  );

                  if (overlaps) {
                    // Check if the min is in an existing group,
                    // don't show the error here if it is as only want
                    // to show the error on the field(s) that require action.
                    return groups.some(
                      group =>
                        this.parent.min >= group.min &&
                        this.parent.min <= group.max,
                    );
                  }
                  /* eslint-enable react/no-this-in-sfc */
                  return true;
                },
              ),
          })}
          onSubmit={(values, helpers) => {
            onAddGroup(values as CustomDataGroup);
            helpers.resetForm();
          }}
        >
          {addForm => (
            <tr>
              <td className="dfe-vertical-align--bottom">
                <FormFieldNumberInput
                  name="min"
                  formGroup={false}
                  hideLabel
                  label={`Min${unitLabel}`}
                  width={10}
                />
              </td>
              <td className="dfe-vertical-align--bottom">
                <FormFieldNumberInput
                  name="max"
                  formGroup={false}
                  hideLabel
                  label={`Max${unitLabel}`}
                  width={10}
                />
              </td>
              <td className="dfe-vertical-align--bottom">
                <Tooltip
                  text={!addForm.isValid ? 'Cannot add invalid group' : ''}
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
                      Add group
                    </Button>
                  )}
                </Tooltip>
              </td>
            </tr>
          )}
        </Formik>
      </tbody>
    </table>
  );
}
