import ButtonText from '@common/components/ButtonText';
import DetailsMenu from '@common/components/DetailsMenu';
import { FormCheckbox, FormTextSearchInput } from '@common/components/form';
import FormField from '@common/components/form/FormField';
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
  disabled?: boolean;
  expandedOptionsList: string[];
  hierarchySearchTerm: string;
  level: number;
  name: string;
  optionTree: FilterHierarchyOption;
  selectedValues: string[];
  toggleOptions: (optionId: string) => void;
}

function FilterHierarchyOptions({
  name,
  optionTree,
  disabled,
  level,
  selectedValues = [],
  expandedOptionsList,
  hierarchySearchTerm,
  toggleOptions,
}: FilterHierarchyOptionsProps) {
  const [searchTerm, setSearchTerm] = useState<string>('');

  const optionLabel = useMemo(() => {
    const termIndex = optionTree.label
      .toLowerCase()
      .indexOf(hierarchySearchTerm);
    if (!hierarchySearchTerm || termIndex === -1) {
      return optionTree.label;
    }

    // text matching search term emboldened
    const regex = new RegExp(hierarchySearchTerm, 'gi');
    const newText = optionTree.label.replace(regex, `<strong>$&</strong>`);
    return (
      // eslint-disable-next-line react/no-danger
      <span dangerouslySetInnerHTML={{ __html: newText }} />
    ) as unknown as Element;
  }, [hierarchySearchTerm, optionTree]);

  const filteredOptions = useMemo(() => {
    if (!optionTree.options) return [];
    if (!hierarchySearchTerm && !searchTerm) return optionTree.options;
    return optionTree.options.filter(option =>
      option.label.toLowerCase().includes(searchTerm.trim().toLowerCase()),
    );
  }, [optionTree, searchTerm, hierarchySearchTerm]);

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
      <div className={styles.checkbox}>
        <FormField
          hint={optionTree.filterLabel}
          label={optionLabel}
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
          hiddenText={`options within ${optionTree.label}`}
        >
          <div>
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
                  {filteredOptions.length} options
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
                  hierarchySearchTerm={hierarchySearchTerm}
                />
              ))}
            </div>
          </div>
        </DetailsMenu>
      )}
    </div>
  );
}

export default memo(FilterHierarchyOptions);
