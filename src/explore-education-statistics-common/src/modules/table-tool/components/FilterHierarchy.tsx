import DetailsMenu from '@common/components/DetailsMenu';
import { FormFieldset } from '@common/components/form';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import { SubjectMetaFilterHierarchy } from '@common/services/tableBuilderService';
import get from 'lodash/get';
import React, { memo, useCallback, useMemo, useState } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import FilterHierarchyOptions, {
  FilterHierarchyOption,
} from './FilterHierarchyOptions';
import { OptionLabelsMap } from './utils/getFilterHierarchyLabelsMap';
import styles from './FilterHierarchy.module.scss';

export interface FilterHierarchyProps {
  optionLabelsMap: OptionLabelsMap;
  filterHierarchy: SubjectMetaFilterHierarchy;
  id?: string;
  disabled?: boolean;
  name: string;
  open?: boolean;
  onToggle?: (isOpen: boolean) => void;
}

function sortAlphabeticalTotalsFirst(a: string, b: string) {
  if (a.toLocaleLowerCase() === 'total') {
    return -1;
  }
  if (b.toLocaleLowerCase() === 'total') {
    return 1;
  }
  if (a < b) {
    return -1;
  }
  if (a > b) {
    return 1;
  }
  return 0;
}

function getRootOptionTrees(
  filterHierarchy: SubjectMetaFilterHierarchy,
  optionLabelsMap: OptionLabelsMap,
): FilterHierarchyOption[] {
  const rootOptionIds = Object.keys(filterHierarchy[0].hierarchy);
  return mapOptionTreesRecursively(
    0,
    rootOptionIds,
    filterHierarchy,
    optionLabelsMap,
  )!;
}

function mapOptionTreesRecursively(
  tier: number,
  optionIds: string[],
  filterHierarchy: SubjectMetaFilterHierarchy,
  optionLabelsMap: OptionLabelsMap,
): FilterHierarchyOption[] | undefined {
  if (optionIds.length === 0) {
    return undefined;
  }
  return optionIds
    .map(optionId => {
      const childOptionIds = filterHierarchy[tier]?.hierarchy[optionId] ?? [];

      return {
        value: optionId,
        label: optionLabelsMap[optionId] ?? '',
        filterLabel: optionLabelsMap[filterHierarchy[tier]?.filterId] ?? '',
        childFilterLabel: optionLabelsMap[filterHierarchy[tier]?.childFilterId],
        options: mapOptionTreesRecursively(
          tier + 1,
          childOptionIds,
          filterHierarchy,
          optionLabelsMap,
        ),
      };
    })
    .filter(
      option => tier === 0 || option.label.toLocaleLowerCase() !== 'total',
    )
    .sort((a, b) => sortAlphabeticalTotalsFirst(a.label, b.label));
}

function FilterHierarchy({
  optionLabelsMap,
  filterHierarchy,
  id: customId,
  name,
  disabled,
  open,
  onToggle,
}: FilterHierarchyProps) {
  const tiersTotal = filterHierarchy.length + 1;

  const filterLabels: string[] = [
    ...filterHierarchy.map(({ filterId }) => optionLabelsMap[filterId ?? '']),
    optionLabelsMap[filterHierarchy.at(-1)?.childFilterId ?? ''],
  ].filter(label => label !== undefined);

  const legend = filterLabels.at(-1) ?? '';

  const rootOptionTrees = useMemo(
    () => getRootOptionTrees(filterHierarchy, optionLabelsMap),
    [filterHierarchy, optionLabelsMap],
  );

  const selectedValues = useWatch({ name });
  const {
    formState: { errors },
  } = useFormContext();

  const { fieldId } = useFormIdContext();
  const id = fieldId(name);
  const errorMessage = get(errors, name)?.message;

  const [expandedOptionsList, setExpandedOptionsList] = useState<string[]>([]);
  const toggleOptions = useCallback(
    (optionId: string) => {
      if (expandedOptionsList.includes(optionId)) {
        return setExpandedOptionsList(
          expandedOptionsList.filter(expandedId => expandedId !== optionId),
        );
      }
      return setExpandedOptionsList([...expandedOptionsList, optionId]);
    },
    [setExpandedOptionsList, expandedOptionsList],
  );

  return (
    <DetailsMenu
      id={`details-${customId ?? id}`}
      open={open}
      jsRequired
      preventToggle={!!errorMessage}
      summary={`${legend}${tiersTotal > 1 ? ` (${tiersTotal} tiers)` : ''}`}
      summaryAfter={<FormCheckboxSelectedCount name={name} />}
      onToggle={onToggle}
    >
      <ul id={id} className={styles.fhList}>
        <FormFieldset
          id={name}
          legend={`Search tiered options of ${legend.toLowerCase()}`}
          legendSize="s"
          useFormId
          error={errorMessage?.toString()}
        >
          {rootOptionTrees.map(optionTree => {
            return (
              <FilterHierarchyOptions
                key={optionTree.value}
                optionTree={optionTree}
                name={name}
                disabled={disabled}
                selectedValues={selectedValues}
                level={0}
                toggleOptions={toggleOptions}
                expandedOptionsList={expandedOptionsList}
              />
            );
          })}
        </FormFieldset>
      </ul>
    </DetailsMenu>
  );
}

export default memo(FilterHierarchy);
