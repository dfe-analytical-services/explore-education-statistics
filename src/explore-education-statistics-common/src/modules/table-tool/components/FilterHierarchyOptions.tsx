import ButtonText from '@common/components/ButtonText';
import DetailsMenu from '@common/components/DetailsMenu';
import { FormCheckbox } from '@common/components/form';
import FormField from '@common/components/form/FormField';
import classNames from 'classnames';
import React, { memo, useCallback, useMemo } from 'react';
import { useFormContext } from 'react-hook-form';
import styles from './FilterHierarchyOptions.module.scss';

export type FilterHierarchyOption = {
  value: string;
  label: string;
  filterLabel: string;
  childFilterLabel?: string;
  options?: FilterHierarchyOption[];
};

interface FilterHierarchyOptionsProps {
  optionTree: FilterHierarchyOption;
  level: number;
  disabled?: boolean;
  name: string;
  selectedValues: string[];
  expandedOptionsList: string[];
  toggleOptions: (optionId: string) => void;
  hideLowerTotals?: boolean;
}

function FilterHierarchyOptions({
  name,
  optionTree,
  disabled,
  level,
  selectedValues = [],
  expandedOptionsList,
  toggleOptions,
  hideLowerTotals = false,
}: FilterHierarchyOptionsProps) {
  const { childOptionIds, hasAllSelected } = useMemo(() => {
    const childIds = optionTree.options?.map(({ value }) => value) ?? [];
    return {
      childOptionIds: childIds,
      hasAllSelected: childIds.every(childOptionId =>
        selectedValues.includes(childOptionId),
      ),
    };
  }, [selectedValues, optionTree]);

  const isExpanded = expandedOptionsList.includes(optionTree.value);
  const { setValue } = useFormContext();

  const toggleSelectAll = useCallback(() => {
    if (hasAllSelected) {
      return setValue(
        name,
        selectedValues.filter(
          selectedValue => !childOptionIds.includes(selectedValue),
        ),
      );
    }
    return setValue(
      name,
      Array.from(new Set([...selectedValues, ...childOptionIds])),
    );
  }, [childOptionIds, setValue, hasAllSelected, selectedValues, name]);

  return level !== 0 &&
    optionTree.label.toLocaleLowerCase() === 'total' &&
    hideLowerTotals ? null : (
    <div
      className={classNames('govuk-checkboxes', {
        'govuk-checkboxes--small': true,
      })}
    >
      <li
        className={classNames(styles.option, {
          [styles.option__expanded]: isExpanded,
        })}
      >
        <div className={styles.checkbox}>
          <FormField
            hint={optionTree.filterLabel}
            label={optionTree.label}
            id={`${name}-${optionTree.value}`}
            // @ts-expect-error  `value` required to make checkboxes unique
            value={optionTree.value}
            name={name}
            as={FormCheckbox}
            checked={selectedValues.includes(optionTree.value)}
            disabled={disabled}
          />
        </div>
        {!!optionTree.options?.length && (
          <DetailsMenu
            summary={`${
              isExpanded ? 'Close' : 'Show'
            } ${optionTree.childFilterLabel?.toLocaleLowerCase()}`}
            open={isExpanded}
            onToggle={() => toggleOptions(optionTree.value)}
            className={styles.detailsMenu}
          >
            <ul className={styles.content}>
              {optionTree.options.length > 1 && (
                <li className={styles.option}>
                  <ButtonText onClick={toggleSelectAll}>
                    {hasAllSelected ? 'Unselect' : 'Select'} all{' '}
                    {optionTree.options.length} options
                  </ButtonText>
                </li>
              )}
              {optionTree.options?.map(childOptionTree => (
                <FilterHierarchyOptions
                  toggleOptions={toggleOptions}
                  expandedOptionsList={expandedOptionsList}
                  optionTree={childOptionTree}
                  key={childOptionTree.value}
                  name={name}
                  disabled={disabled}
                  selectedValues={selectedValues}
                  level={level + 1}
                />
              ))}
            </ul>
          </DetailsMenu>
        )}
      </li>
    </div>
  );
}

export default memo(FilterHierarchyOptions);
