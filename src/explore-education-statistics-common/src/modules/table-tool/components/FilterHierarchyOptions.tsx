import { FormCheckbox } from '@common/components/form';
import FormField from '@common/components/form/FormField';
import classNames from 'classnames';
import React, { memo } from 'react';

export type FilterHierarchyOption = {
  value: string;
  label: string;
  options?: FilterHierarchyOption[];
};

interface FilterHierarchyOptionsProps {
  disabled?: boolean;
  id?: string;
  level?: number;
  name: string;
  options?: FilterHierarchyOption[];
  totalColumns: number;
  value: string[];
  hideLowerTotals?: boolean;
}

function FilterHierarchyOptions({
  disabled,
  id,
  level = 0,
  options,
  totalColumns,
  value = [],
  hideLowerTotals = true,
  ...props
}: FilterHierarchyOptionsProps) {
  if (!options) {
    return undefined;
  }

  return (
    <div
      className={classNames('govuk-checkboxes', {
        'govuk-checkboxes--small': true,
      })}
    >
      {options.map(option => {
        const key = `${id ? `${id}.` : ``}${option.value}`;
        return (
          <div
            id={id}
            key={key}
            className="govuk-!-margin-left-8 govuk-!-margin-bottom-2"
          >
            {hideLowerTotals &&
            level !== 0 &&
            option.label.toLocaleLowerCase() === 'total' &&
            hideLowerTotals ? null : (
              <FormField
                {...option}
                name={props.name}
                as={FormCheckbox}
                id={key}
                checked={!!option.value && value.includes(option.value)}
                disabled={disabled}
              />
            )}
            <FilterHierarchyOptions
              {...props}
              id={key}
              options={option.options}
              level={level + 1}
              totalColumns={totalColumns}
              value={value}
            />
          </div>
        );
      })}
    </div>
  );
}

export default memo(FilterHierarchyOptions);
