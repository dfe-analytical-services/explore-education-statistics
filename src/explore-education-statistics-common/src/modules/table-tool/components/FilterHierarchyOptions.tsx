import { FormCheckbox } from '@common/components/form';
import FormField from '@common/components/form/FormField';
import classNames from 'classnames';
import React, { memo } from 'react';

export type FilterHierarchyOptionsTree<T=string> = {
  value: T;
  label: string;
  options?: FilterHierarchyOptionsTree<T>;
}[];

interface FilterHierarchyOptionsProps {
  disabled?: boolean;
  id?: string;
  level?: number;
  name: string;
  optionsTree?: FilterHierarchyOptionsTree;
  totalColumns: number;
  value: string[];
  hideLowerTotals?: boolean;
}

export const filterHierarchySeparator = ', ';

function FilterHierarchyOptions({
  disabled,
  id,
  level = 0,
  optionsTree,
  totalColumns,
  value = [],
  hideLowerTotals = true,
  ...props
}: FilterHierarchyOptionsProps) {
  if (!optionsTree) {
    return undefined;
  }

  return (
    <div
      className={classNames('govuk-checkboxes', {
        'govuk-checkboxes--small': true,
      })}
    >
      {optionsTree.map(option => {
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
                label={`${option.label} (${
                  option.value.split(filterHierarchySeparator).length
                }) ${option.value}`}
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
              optionsTree={option.options}
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
