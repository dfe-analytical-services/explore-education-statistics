import ButtonText from '@common/components/ButtonText';
import { CheckboxOption } from '@common/components/form/FormCheckboxGroup';
import styles from '@common/components/form/FormCheckboxGroup.module.scss';
import { FormFieldset } from '@common/components/form';
import difference from 'lodash/difference';
import React, { useMemo, ReactNode } from 'react';

interface RHFCheckboxGroupProps {
  children: ReactNode;
  id: string;
  legend: string;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  options: CheckboxOption[];
  values: string[];
  onToggleSelectAll: (nextValues: string[]) => void;
}

const RHFCheckboxGroup = ({
  children,
  id,
  legend,
  legendSize = 's',
  options,
  values = [],
  onToggleSelectAll,
}: RHFCheckboxGroupProps) => {
  const isAllSelected = useMemo(
    () => options.every(option => values && values.includes(option.value)),
    [options, values],
  );
  return (
    <FormFieldset id={id} legend={legend} legendSize={legendSize}>
      {options.length > 1 && (
        <ButtonText
          className={styles.selectAll}
          underline={false}
          onClick={() => {
            const allOptionValues = options.map(option => option.value);
            const restValues = Array.isArray(values)
              ? difference(values, allOptionValues)
              : [];

            const nextValues = isAllSelected
              ? restValues
              : [...restValues, ...allOptionValues];
            onToggleSelectAll(nextValues);
          }}
        >
          {isAllSelected
            ? `Unselect all ${options.length} subgroup options`
            : `Select all ${options.length} subgroup options`}
        </ButtonText>
      )}
      <div className="govuk-checkboxes govuk-checkboxes--small">{children}</div>
    </FormFieldset>
  );
};

export default RHFCheckboxGroup;
