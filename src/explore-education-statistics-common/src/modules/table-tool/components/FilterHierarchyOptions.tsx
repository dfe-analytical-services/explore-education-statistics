import ButtonText from '@common/components/ButtonText';
import ContentHtml from '@common/components/ContentHtml';
import { FormCheckbox, FormTextSearchInput } from '@common/components/form';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import getFilterOptionIdsRecursively from '@common/modules/table-tool/utils/getFilterOptionIdsRecursively';
import FilterAccordion from '@common/modules/table-tool/components/FilterAccordion';
import styles from '@common/modules/table-tool/components/FilterHierarchyOptions.module.scss';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React, { memo, useCallback, useMemo, useState } from 'react';
import { useFormContext } from 'react-hook-form';

export type FilterHierarchyOption = {
  value: string;
  label: string;
  filterLabel: string;
  childFilterLabel?: string;
  options?: FilterHierarchyOption[];
};

export type SelectedChildren = {
  valuesRelatedToSelectedValues: string[];
  valuesRelatedToSelectedValuesCountMap: Dictionary<number>;
};

interface FilterHierarchyOptionsProps {
  disabled?: boolean;
  expandedOptionsList?: string[];
  hierarchySearchTerm?: string;
  level: number;
  name: string;
  optionTree: FilterHierarchyOption;
  selectedValues?: string[];
  selectedChildren: SelectedChildren;
  onToggleOptions: (optionId: string) => void;
}

function FilterHierarchyOptions({
  name,
  optionTree,
  disabled,
  level,
  selectedValues = [],
  expandedOptionsList = [],
  hierarchySearchTerm = '',
  selectedChildren,
  onToggleOptions,
}: FilterHierarchyOptionsProps) {
  const [searchTerm, setSearchTerm] = useState<string>('');

  const renderSelectedCount = useCallback(() => {
    const selectedChildrenCount =
      selectedChildren.valuesRelatedToSelectedValuesCountMap[optionTree.value];

    if (!selectedChildrenCount) {
      return null;
    }
    return (
      <Tag className="govuk-!-margin-left-2 govuk-!-font-size-16">
        {selectedChildrenCount} selected
      </Tag>
    );
  }, [
    optionTree.value,
    selectedChildren.valuesRelatedToSelectedValuesCountMap,
  ]);

  const renderLabel = useCallback(
    (label: string) => {
      const termIndex = label.toLowerCase().indexOf(hierarchySearchTerm);
      if (!hierarchySearchTerm || termIndex === -1) {
        return optionTree.label;
      }

      // text matching search term emboldened
      const regex = new RegExp(hierarchySearchTerm, 'gi');
      const labelHtml = optionTree.label.replace(regex, `<em>$&</em>`);
      return (
        <ContentHtml
          testId="search-highlight"
          html={labelHtml}
          sanitizeOptions={{ allowedTags: ['em'] }}
        />
      );
    },
    [hierarchySearchTerm, optionTree],
  );

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

  const { thisAndAllChildOptionIds, hasThisAndAllChildOptionsSelected } =
    useMemo(() => {
      const allOptionIds =
        filteredOptions.flatMap(getFilterOptionIdsRecursively) ?? [];
      const allIds = [optionTree.value, ...allOptionIds];

      return {
        thisAndAllChildOptionIds: allIds,
        hasThisAndAllChildOptionsSelected: allIds.every(childOptionId =>
          selectedValues.includes(childOptionId),
        ),
      };
    }, [filteredOptions, optionTree.value, selectedValues]);

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

  const toggleSelectThisAndSubCategories = useCallback(() => {
    if (hasThisAndAllChildOptionsSelected) {
      return setValue(
        name,
        selectedValues.filter(
          selectedValue => !thisAndAllChildOptionIds.includes(selectedValue),
        ),
      );
    }
    return setValue(
      name,
      Array.from(new Set([...selectedValues, ...thisAndAllChildOptionIds])),
    );
  }, [
    hasThisAndAllChildOptionsSelected,
    name,
    selectedValues,
    setValue,
    thisAndAllChildOptionIds,
  ]);

  return (
    <div
      data-testid={`filter-hierarchy-options-${optionTree.value}`}
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
          renderLabel={renderLabel}
          value={optionTree.value}
          hint={optionTree.filterLabel}
          disabled={disabled}
          checked={selectedValues.includes(optionTree.value)}
        />
      </div>
      {!!optionTree.options?.length && (
        <FilterAccordion
          label={`${
            isExpanded ? 'Close this sub category' : 'Show sub categories'
          }`}
          labelAfter={
            <>
              {!!optionTree.options?.length && isExpanded && (
                <>
                  <span aria-hidden className={styles.divider}>
                    |
                  </span>
                  <ButtonText
                    className={styles.selectThisAndAllButton}
                    underline={false}
                    onClick={toggleSelectThisAndSubCategories}
                  >
                    {`${
                      hasThisAndAllChildOptionsSelected ? 'Unselect' : 'Select'
                    } this and all sub categories`}
                    <VisuallyHidden>{` for ${optionTree.label.toLocaleLowerCase()}`}</VisuallyHidden>
                  </ButtonText>
                </>
              )}
              {renderSelectedCount()}
            </>
          }
          labelHiddenText={` for ${optionTree.label.toLocaleLowerCase()}`}
          id={`${name}-${optionTree.value}-options`}
          open={isExpanded}
          onToggle={() => onToggleOptions(optionTree.value)}
        >
          {level !== 0 &&
            optionTree.options.length > 6 &&
            !hierarchySearchTerm && (
              <div className={styles.search}>
                <FormTextSearchInput
                  id={`${name}-${optionTree.value}-search`}
                  name={`${name}-search`}
                  label={`Search ${optionTree.label}`}
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
                {hasAllSelected ? 'Unselect all' : 'Select all'}
                <VisuallyHidden>
                  {` sub categories for ${optionTree.label.toLocaleLowerCase()}`}
                </VisuallyHidden>
              </ButtonText>
            )}
            {filteredOptions?.map(childOptionTree => (
              <FilterHierarchyOptions
                expandedOptionsList={expandedOptionsList}
                optionTree={childOptionTree}
                key={childOptionTree.value}
                name={name}
                disabled={disabled}
                selectedValues={selectedValues}
                level={level + 1}
                hierarchySearchTerm={hierarchySearchTerm}
                selectedChildren={selectedChildren}
                onToggleOptions={onToggleOptions}
              />
            ))}
          </div>
        </FilterAccordion>
      )}
    </div>
  );
}

export default memo(FilterHierarchyOptions);
