import ButtonText from '@common/components/ButtonText';
import DetailsMenu from '@common/components/DetailsMenu';
import { FormCheckbox } from '@common/components/form';
import FormField from '@common/components/form/FormField';
import { SubjectMetaFilterHierarchy } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React, { memo, useCallback, useMemo } from 'react';
import { useFormContext } from 'react-hook-form';
import styles from './FilterHierarchyOptions.module.scss';

export type FilterHierarchyOption2 = {
  value: string;
  label: string;
  options?: FilterHierarchyOption2[];
};

interface FilterHierarchyOptionsProps {
  optionLabelsMap: Dictionary<string>;
  filterHierarchy: SubjectMetaFilterHierarchy;
  filterLabels: string[];
  tiersTotal: number;
  optionId: string;
  level: number;
  disabled?: boolean;
  name: string;
  selectedValues: string[];
  expandedOptionsList: string[];
  toggleOptions: (optionId: string) => void;
  hideLowerTotals?: boolean;
}

function FilterHierarchyOptions({
  optionId,
  optionLabelsMap,
  filterHierarchy,
  filterLabels,
  tiersTotal,
  name,
  disabled,
  level,
  selectedValues = [],
  expandedOptionsList,
  toggleOptions,
  hideLowerTotals = false,
}: FilterHierarchyOptionsProps) {
  const { optionLabel, childOptionIds, hasAllSelected } = useMemo(() => {
    const optionIds = filterHierarchy[level]?.hierarchy[optionId] ?? [];

    return {
      optionLabel: optionLabelsMap[optionId],
      childOptionIds: optionIds,
      hasAllSelected: optionIds.every(childOptionid =>
        selectedValues.includes(childOptionid),
      ),
    };
  }, [selectedValues, optionLabelsMap, filterHierarchy, level, optionId]);

  const isExpanded = expandedOptionsList.includes(optionId);
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
  }, [childOptionIds, setValue, hasAllSelected, selectedValues]);

  return level !== 0 &&
    optionLabel.toLocaleLowerCase() === 'total' &&
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
            hint={filterLabels[level]}
            label={optionLabel}
            id={`${name}-${optionId}`}
            // @ts-expect-error  `value` required to make checkboxes unique
            value={optionId}
            name={name}
            as={FormCheckbox}
            checked={selectedValues.includes(optionId)}
            disabled={disabled}
          />
        </div>
        {!!childOptionIds.length && (
          <DetailsMenu
            summary={`${isExpanded ? 'Close' : 'Show'} ${filterLabels[
              level + 1
            ]?.toLocaleLowerCase()}`}
            open={isExpanded}
            onToggle={() => toggleOptions(optionId)}
            className={styles.detailsMenu}
          >
            <ul className={styles.content}>
              {childOptionIds.length > 1 && (
                <li className={styles.option}>
                  <ButtonText onClick={toggleSelectAll}>
                    {hasAllSelected ? 'Unselect' : 'Select'} all{' '}
                    {childOptionIds.length} options
                  </ButtonText>
                </li>
              )}
              {childOptionIds.map(childOptionId => (
                <FilterHierarchyOptions
                  toggleOptions={toggleOptions}
                  expandedOptionsList={expandedOptionsList}
                  optionId={childOptionId}
                  key={childOptionId}
                  optionLabelsMap={optionLabelsMap}
                  filterHierarchy={filterHierarchy}
                  filterLabels={filterLabels}
                  name={name}
                  disabled={disabled}
                  tiersTotal={tiersTotal}
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
