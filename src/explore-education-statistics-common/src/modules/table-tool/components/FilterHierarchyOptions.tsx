import ButtonText from '@common/components/ButtonText';
import DetailsMenu from '@common/components/DetailsMenu';
import { FormCheckbox, FormTextSearchInput } from '@common/components/form';
import VisuallyHidden from '@common/components/VisuallyHidden';
import classNames from 'classnames';
import React, { memo, useCallback, useMemo, useState } from 'react';
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
}

function FilterHierarchyOptions({
  name,
  optionTree,
  disabled,
  level,
  selectedValues = [],
  expandedOptionsList,
  toggleOptions,
}: FilterHierarchyOptionsProps) {
  const [searchTerm, setSearchTerm] = useState<string>('');

  const filteredOptions = useMemo(() => {
    if (!optionTree.options) return [];
    if (!searchTerm) return optionTree.options;
    return optionTree.options.filter(option =>
      option.label.toLowerCase().includes(searchTerm.trim().toLowerCase()),
    );
  }, [optionTree, searchTerm]);

  const { childOptionIds, hasAllSelected } = useMemo(() => {
    const childIds = filteredOptions?.map(({ value }) => value) ?? [];
    return {
      childOptionIds: childIds,
      hasAllSelected: childIds.every(childOptionId =>
        selectedValues.includes(childOptionId),
      ),
    };
  }, [selectedValues, filteredOptions]);

  const isExpanded = expandedOptionsList.includes(optionTree.value);
  const { setValue, register } = useFormContext();
  const { ref: inputRef, ...field } = register(name);

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

  return (
    <div
      className={classNames(
        'govuk-checkboxes',
        'govuk-checkboxes--small',
        styles.option,
        {
          [styles.option__expanded]: isExpanded,
        },
      )}
    >
      <div className={styles.checkboxContainer}>
        <FormCheckbox
          {...field}
          inputRef={inputRef}
          key={optionTree.label}
          id={`${name}-${optionTree.value}`}
          label={optionTree.label}
          value={optionTree.value}
          hint={optionTree.filterLabel}
          disabled={disabled}
          checked={selectedValues.includes(optionTree.value)}
        />
      </div>
      {!!optionTree.options?.length && (
        <DetailsMenu
          summary={`${
            isExpanded ? 'Close' : 'Show'
          } ${optionTree.childFilterLabel?.toLocaleLowerCase()}`}
          hiddenText={`options for ${optionTree.label.toLocaleLowerCase()}`}
          open={isExpanded}
          onToggle={() => toggleOptions(optionTree.value)}
          className={styles.detailsMenu}
        >
          {optionTree.options.length > 6 && (
            <div className={styles.search}>
              <FormTextSearchInput
                id={`${name}-search`}
                name={`${name}-search`}
                label={`Search ${
                  optionTree.label
                } (${optionTree.childFilterLabel?.toLocaleLowerCase()})`}
                width={20}
                onChange={event => setSearchTerm(event.target.value)}
                onKeyPress={event => {
                  if (event.key === 'Enter') {
                    event.preventDefault();
                  }
                }}
              />
            </div>
          )}
          <div>
            {filteredOptions.length > 1 && (
              <ButtonText
                className={styles.selectAllButton}
                onClick={toggleSelectAll}
              >
                {hasAllSelected ? 'Unselect' : 'Select'} all{' '}
                {filteredOptions.length} <span aria-hidden>options</span>
                <VisuallyHidden>
                  {' '}
                  {optionTree.childFilterLabel!.toLowerCase()} options for{' '}
                  {optionTree.label.toLocaleLowerCase()}
                </VisuallyHidden>
              </ButtonText>
            )}
            {filteredOptions?.map(childOptionTree => (
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
          </div>
        </DetailsMenu>
      )}
    </div>
  );
}

export default memo(FilterHierarchyOptions);
