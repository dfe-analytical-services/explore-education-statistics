import ButtonText from '@common/components/ButtonText';
import DetailsMenu from '@common/components/DetailsMenu';
import { FormFieldset } from '@common/components/form';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import FormSearchBar from '@common/components/form/FormSearchBar';
import Modal from '@common/components/Modal';
import { SubjectMetaFilterHierarchy } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import sortAlphabeticalTotalsFirst from '@common/utils/sortAlphabeticalTotalsFirst';
import classNames from 'classnames';
import get from 'lodash/get';
import React, { memo, useCallback, useEffect, useMemo, useState } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import styles from './FilterHierarchy.module.scss';
import FilterHierarchyOptions, {
  FilterHierarchyOption,
  SelectedChildren,
} from './FilterHierarchyOptions';
import {
  isOptionTotal,
  OptionLabelsMap,
} from './utils/getFilterHierarchyLabelsMap';
import augmentFilterHierarchySelections from './utils/augmentFilterHierarchySelections';
import {
  hierarchyOptionsFromString,
  hierarchyOptionsToString,
} from './utils/filterHierarchiesConversion';

export interface FilterHierarchyProps {
  optionLabelsMap: OptionLabelsMap;
  filterHierarchy: SubjectMetaFilterHierarchy;
  id?: string;
  disabled?: boolean;
  name: string;
  open?: boolean;
  onToggle?: (isOpen: boolean) => void;
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
  const {
    formState: { errors },
  } = useFormContext();

  const { fieldId } = useFormIdContext();
  const id = fieldId(name);
  const errorMessage = get(errors, name)?.message;

  const tiersTotal = filterHierarchy.length + 1;
  const [expandedOptionsList, setExpandedOptionsList] = useState<string[]>([]);

  const filterLabels: string[] = [
    ...filterHierarchy.map(({ filterId }) => optionLabelsMap[filterId ?? '']),
    optionLabelsMap[filterHierarchy.at(-1)?.childFilterId ?? ''],
  ].filter(label => label !== undefined);

  const legend = filterLabels.at(-1) ?? '';

  const [hierarchySearchTerm, setHierarchySearchTerm] = useState('');
  const searchFormSubmit = useCallback(
    (value: string) => {
      setHierarchySearchTerm(value.toLowerCase().trim());
    },
    [setHierarchySearchTerm],
  );

  const searchFormReset = useCallback(() => {
    setExpandedOptionsList([]);
    setHierarchySearchTerm('');
  }, [setHierarchySearchTerm, setExpandedOptionsList]);

  const expandAllOptions = useCallback(
    (optionTrees: FilterHierarchyOption[]) => {
      function getOptionIdsRecursively(
        optionTree: FilterHierarchyOption,
      ): string[] {
        const optionsIds =
          optionTree.options?.flatMap(getOptionIdsRecursively) ?? [];
        return [optionTree.value, ...optionsIds];
      }
      setExpandedOptionsList(optionTrees.flatMap(getOptionIdsRecursively));
    },
    [setExpandedOptionsList],
  );

  const { rootOptionTrees, searchLegend } = useMemo(() => {
    const optionTrees = getRootOptionTrees(
      filterHierarchy,
      optionLabelsMap,
      hierarchySearchTerm,
    );
    if (optionTrees.length === 0) {
      return {
        rootOptionTrees: [],
        searchLegend: `No options found, searching '${hierarchySearchTerm}' in all tiers of ${legend.toLowerCase()}, please expand your search`,
      };
    }

    if (hierarchySearchTerm) {
      expandAllOptions(optionTrees);
    } else {
      setExpandedOptionsList([]);
    }

    const searchHint = hierarchySearchTerm
      ? `Searching '${hierarchySearchTerm}' in all tiers of ${legend.toLowerCase()}`
      : `Browse all tiers of ${legend.toLowerCase()}`;

    return { rootOptionTrees: optionTrees, searchLegend: searchHint };
  }, [
    filterHierarchy,
    optionLabelsMap,
    hierarchySearchTerm,
    legend,
    expandAllOptions,
  ]);

  const getOptionUniqueValue = useCallback(
    (optionValue: string): string => {
      return hierarchyOptionsToString(
        augmentFilterHierarchySelections(
          { [name]: [optionValue] },
          [filterHierarchy],
          optionLabelsMap,
        )[name][optionValue],
      );
    },
    [filterHierarchy, name, optionLabelsMap],
  );

  const selectedValues = useWatch({ name }) as string[];
  const optionsWithSelectedChildren: SelectedChildren = useMemo(() => {
    if (!selectedValues?.length) {
      return {
        valuesRelatedToSelectedValues: [],
        valuesRelatedToSelectedValuesCountMap: {},
      };
    }

    const tierTotals = filterHierarchy.map(hierarchyTier => {
      return Object.values(hierarchyTier.hierarchy)
        .flat()
        .find(optionId => isOptionTotal(optionLabelsMap, optionId))!;
    });

    // includes duplicates
    const ancestorsWithSelectedChildren = selectedValues
      .map((optionValue: string) => {
        // find related values that aren't totals
        const nonTotalRelatedOptions = hierarchyOptionsFromString(
          optionValue,
        ).filter((optionId: string) => !tierTotals.includes(optionId));

        // remove bottom value, as this is the selected options, we want ancestors only
        const relatedAncestors = nonTotalRelatedOptions.slice(0, -1);

        return relatedAncestors.map(getOptionUniqueValue);
      })
      .flat();

    // count duplicates, store them in a dictionary keyed by their unique value, with count as value
    const valuesRelatedToSelectedValuesCountMap = ancestorsWithSelectedChildren
      .flat()
      .reduce((acc: Dictionary<number>, item: string) => {
        acc[item] = (acc[item] || 0) + 1;
        return acc;
      }, {});

    // remove all duplicates
    const uniqueRelatedValues = Array.from(
      new Set([...ancestorsWithSelectedChildren]),
    );

    return {
      valuesRelatedToSelectedValues: uniqueRelatedValues,
      valuesRelatedToSelectedValuesCountMap,
    };
  }, [selectedValues, filterHierarchy, optionLabelsMap, getOptionUniqueValue]);

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

  const expandSelectedOptions = useCallback(() => {
    return setExpandedOptionsList(
      optionsWithSelectedChildren.valuesRelatedToSelectedValues,
    );
  }, [setExpandedOptionsList, optionsWithSelectedChildren]);

  useEffect(() => {
    expandSelectedOptions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Groups with an error are opened, so add them to the list of open
  // filters to prevent the group collapsing as soon as you select
  // an option in the group.
  useEffect(() => {
    if (!open && get(errors, name)) {
      onToggle?.(true);
    }
  }, [errors, name, open, onToggle]);

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
      <Modal
        title={`${legend} tiers`}
        triggerButton={
          <ButtonText className="govuk-!-margin-bottom-5">
            What are {legend.toLocaleLowerCase()} tiers?
          </ButtonText>
        }
        showClose
        closeText="Close"
      >
        {filterLabels.map((filterLabel, index) => {
          const isFirst = index === 0;
          const isLast = index + 1 === filterLabels.length;

          return (
            <div
              key={filterLabel}
              className={classNames(styles.tier, {
                [styles.subTier]: !isFirst,
              })}
              data-testid="modal-tier-description-section"
            >
              <h3 className="govuk-heading-s">
                {filterLabel} (tier {index + 1})
              </h3>
              {isFirst && !isLast && (
                <>
                  <p>This is a top level category.</p>
                  <p>
                    Selecting a tier 1 option provides a total value for all sub
                    categories associated to this area
                  </p>
                </>
              )}
              {!isFirst && !isLast && (
                <>
                  <p>This is a sub category of tier {index}.</p>
                  <p>
                    Selecting a tier {index + 1} option provides a total value
                    for all {filterLabels[index + 1].toLowerCase()} options
                    associated to this area
                  </p>
                </>
              )}
              {isLast && (
                <>
                  <p>These are the individual {filterLabel.toLowerCase()}</p>
                  <p>
                    If you're not sure about a specific{' '}
                    {filterLabel.toLowerCase()} options, then you can also use
                    the browse feature to find via related parent tiers
                  </p>
                </>
              )}
            </div>
          );
        })}
      </Modal>
      <FormSearchBar
        id={`${name}-search`}
        name={`${name}-search`}
        labelSize="s"
        label={`Search all tiers and ${legend.toLowerCase()}`}
        className={styles.search}
        onSubmit={searchFormSubmit}
        min={0}
        onReset={searchFormReset}
      />
      <FormFieldset
        id={id}
        legend={searchLegend}
        legendSize="s"
        useFormId={false}
        error={errorMessage?.toString()}
      >
        <div data-testid="filter-hierarchy-options">
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
                hierarchySearchTerm={hierarchySearchTerm}
                selectedChildren={optionsWithSelectedChildren}
              />
            );
          })}
        </div>
      </FormFieldset>
    </DetailsMenu>
  );
}

function getRootOptionTrees(
  filterHierarchy: SubjectMetaFilterHierarchy,
  optionLabelsMap: OptionLabelsMap,
  hierarchySearchTerm: string,
): FilterHierarchyOption[] {
  const rootOptionIds = Object.keys(filterHierarchy[0].hierarchy);

  const tierTotals = filterHierarchy.map(hierarchyTier => {
    return Object.values(hierarchyTier.hierarchy)
      .flat()
      .find(optionId => isOptionTotal(optionLabelsMap, optionId))!;
  });

  return mapOptionTreesRecursively({
    tier: 0,
    optionIds: rootOptionIds,
    filterHierarchy,
    optionLabelsMap,
    tierTotals,
    hierarchySearchTerm,
    ancestorIds: [],
  })!;
}

function mapOptionTreesRecursively({
  tier,
  optionIds,
  filterHierarchy,
  optionLabelsMap,
  tierTotals,
  hierarchySearchTerm,
  ancestorIds = [],
}: {
  tier: number;
  optionIds: string[];
  filterHierarchy: SubjectMetaFilterHierarchy;
  optionLabelsMap: OptionLabelsMap;
  tierTotals: string[];
  hierarchySearchTerm: string;
  ancestorIds: string[];
}): FilterHierarchyOption[] | undefined {
  if (optionIds.length === 0) {
    return undefined;
  }
  const unfilteredOptionsTree = optionIds.map(optionId => {
    const childOptionIds = filterHierarchy[tier]?.hierarchy[optionId] ?? [];

    // join ancestor ids, option id and child total ids into one unique option value string
    const optionValue = hierarchyOptionsToString([
      ...ancestorIds,
      optionId,
      ...tierTotals.slice(tier),
    ]);

    return {
      value: optionValue,
      label: optionLabelsMap[optionId]?.trim() ?? '',
      filterLabel: optionLabelsMap[filterHierarchy[tier]?.filterId] ?? '',
      childFilterLabel: optionLabelsMap[filterHierarchy[tier]?.childFilterId],
      options: mapOptionTreesRecursively({
        tier: tier + 1,
        optionIds: childOptionIds,
        filterHierarchy,
        optionLabelsMap,
        tierTotals,
        hierarchySearchTerm,
        ancestorIds: [...ancestorIds, optionId],
      }),
    };
  });

  const optionsTree = unfilteredOptionsTree
    .filter(
      option => tier === 0 || option.label.toLocaleLowerCase() !== 'total',
    )
    .sort((a, b) => sortAlphabeticalTotalsFirst(a.label, b.label));

  if (!hierarchySearchTerm) {
    return optionsTree;
  }

  // filter option tree options based on search term
  return optionsTree.filter(option => {
    const hasOptions = (option.options?.length ?? 0) > 0;
    const hasSearchTerm = option.label
      .toLowerCase()
      .includes(hierarchySearchTerm.trim().toLowerCase());
    return hasOptions || hasSearchTerm;
  });
}

export default memo(FilterHierarchy);
