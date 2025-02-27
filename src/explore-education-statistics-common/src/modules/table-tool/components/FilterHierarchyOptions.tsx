import { FormCheckbox, FormFieldCheckbox } from '@common/components/form';
import useRegister from '@common/components/form/hooks/useRegister';
import classNames from 'classnames';
import { useFormContext } from 'react-hook-form';
import getFilterHierarchyColumns from '../utils/getFilterHierarchyColumns';
import React, { memo } from 'react';
import FormField from '@common/components/form/FormField';

export type FilterHierarchyOptionsTree = {
  value: string;
  label: string;
  options?: FilterHierarchyOptionsTree;
}[];

interface FilterHierarchyOptionsProps {
  disabled?: boolean;
  id: string;
  level?: number;
  name: string;
  optionsTree?: FilterHierarchyOptionsTree;
  totalColumns: number;
  value: string[];
}

function FilterHierarchyOptions({
  disabled,
  id,
  level = 0,
  optionsTree,
  totalColumns,
  value = [],
  ...props
}: FilterHierarchyOptionsProps) {
  if (!optionsTree) {
    return undefined;
  }

  return (
    <div
      className={classNames(
        'govuk-checkboxes',
        {
          'govuk-checkboxes--small': true,
        },
        getFilterHierarchyColumns(totalColumns - level, totalColumns),
      )}
    >
      {level === 0 && JSON.stringify({ name: props.name, value })}
      {optionsTree.map(option => {
        const key = `${id}.${option.value}`;
        return (
          <div key={key} className="govuk-grid-row govuk-!-margin-bottom-2">
            <div className={getFilterHierarchyColumns(1, totalColumns - level)}>
              <FormField
                name={props.name}
                as={FormCheckbox}
                {...option}
                id={key}
              />
            </div>
            <FilterHierarchyOptions
              {...props}
              id={key}
              optionsTree={option.options}
              level={level + 1}
              totalColumns={totalColumns}
              value={value}
            />
          </div>
        );
      })}
      {level !== 0 && <hr />}
    </div>
  );
}

export default memo(FilterHierarchyOptions);
