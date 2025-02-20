import type {
  SubjectMetaFilterHierarchy,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import React, { memo, useCallback, useMemo } from 'react';
import { Dictionary } from '@common/types';
import sortBy from 'lodash/sortBy';
import FilterHierarchyOptions, {
  FilterHierarchyOptionsTree,
} from './FilterHierarchyOptions';
import getFilterHierarchyColumns from '../utils/getFilterHierarchyColumns';
import {
  FormCheckbox,
  FormFieldCheckbox,
  FormFieldCheckboxGroup,
  FormFieldCheckboxSearchSubGroups,
  FormFieldset,
} from '@common/components/form';
import DetailsMenu from '@common/components/DetailsMenu';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import WarningMessage from '@common/components/WarningMessage';
import ButtonText from '@common/components/ButtonText';
import { useFormContext, useWatch } from 'react-hook-form';
import useRegister from '@common/components/form/hooks/useRegister';
import { FiltersFormValues } from './FiltersForm';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormFieldCheckboxGroupsMenu from '@common/components/form/FormFieldCheckboxGroupsMenu';
import FilterHierarchyFormFieldOptions from './FilterHierarchyFormFieldOptions';

interface Props {
  disabled?: boolean;
  filterHierarchy: SubjectMetaFilterHierarchy;
  filters: SubjectMeta['filters'];
  id?: string;
  filterId: string;
  open?: boolean;
  totalTiers: number;
  onToggle?: (isOpen: boolean) => void;
}

function FilterHierarchy({
  disabled,
  filters,
  filterHierarchy,
  id: customId,
  filterId,
  open = false,
  totalTiers,
  onToggle,
}: Props) {
  const [labels, autoSelectIds] = useMemo(() => {
    const labelsMap: Dictionary<string> = {};
    const autoSelectFilterItemIds: string[] = [];

    Object.values(filters).forEach(filter => {
      if (filter.autoSelectFilterItemId) {
        autoSelectFilterItemIds.push(filter.autoSelectFilterItemId);
      }
      labelsMap[filter.id] = filter.legend;
      Object.values(filter.options).forEach(filterGroups => {
        labelsMap[filterGroups.id] = filterGroups.label;
        filterGroups.options.forEach(filterGroupOption => {
          labelsMap[filterGroupOption.value] = filterGroupOption.label;
        });
      });
    });

    return [labelsMap, autoSelectFilterItemIds];
  }, [filters]);

  const name: `filterHierarchies.${string}` = 'filterHierarchies.one'; //`filterHierarchies.${filterId}`;
  const { fieldId } = useFormIdContext();
  const id = fieldId(name, customId);

  const sortOptions = useCallback(
    (options: string[]) => [
      ...options.filter(a => {
        return (
          autoSelectIds.indexOf(a) !== -1 ||
          labels[a].toLocaleLowerCase() === 'total'
        );
      }),
      ...options.filter(a => {
        return (
          autoSelectIds.indexOf(a) === -1 &&
          labels[a].toLocaleLowerCase() !== 'total'
        );
      }),
    ],
    [autoSelectIds],
  );

  const filterHierarchyOptions = useMemo(() => {
    const tiers = sortBy(filterHierarchy.tiers, 'level');

    function getTierOptions({
      currentLevel = 0,
      currentOptionId,
    }: {
      currentOptionId: string;
      currentLevel?: number;
    }): FilterHierarchyOptionsTree | undefined {
      const isBottomLevel = currentLevel === totalTiers - 1;
      if (isBottomLevel) {
        return undefined;
      }

      const tierOptions = tiers[currentLevel]?.hierarchy[currentOptionId] ?? [];

      return sortOptions(tierOptions).map(childOptionId => ({
        value: childOptionId,
        label: labels[childOptionId],
        options: getTierOptions({
          currentLevel: currentLevel + 1,
          currentOptionId: childOptionId,
        }),
      }));
    }

    return sortOptions(filterHierarchy.rootOptionIds).map(
      firstLevelOptionId => {
        return {
          value: firstLevelOptionId,
          label: labels[firstLevelOptionId],
          options: getTierOptions({ currentOptionId: firstLevelOptionId }),
        };
      },
    );
  }, [filterHierarchy, labels, totalTiers]);

  return (
    <DetailsMenu
      id={`details-${customId ?? id}`}
      open={open}
      jsRequired
      summary={`${labels[filterHierarchy.rootFilterId]} ${
        totalTiers > 1 ? `(${totalTiers} tiers)` : ''
      }`}
      summaryAfter={<FormCheckboxSelectedCount name={'name'} />}
      onToggle={onToggle}
    >
      <h4>Browse qualifications by related tiers</h4>
      <FilterHierarchyFormFieldOptions
        disabled={disabled}
        name={name}
        optionsTree={filterHierarchyOptions}
        totalColumns={totalTiers}
      />
    </DetailsMenu>
  );
}

export default memo(FilterHierarchy);
