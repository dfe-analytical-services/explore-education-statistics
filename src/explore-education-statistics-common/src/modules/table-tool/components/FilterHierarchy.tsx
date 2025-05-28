import ButtonText from '@common/components/ButtonText';
import DetailsMenu from '@common/components/DetailsMenu';
import { FormFieldset } from '@common/components/form';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import Modal from '@common/components/Modal';
import { SubjectMetaFilterHierarchy } from '@common/services/tableBuilderService';
import sortAlphabeticalTotalsFirst from '@common/utils/sortAlphabeticalTotalsFirst';
import classNames from 'classnames';
import get from 'lodash/get';
import React, { memo, useCallback, useEffect, useMemo, useState } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import FormSearchBar from '@common/components/form/FormSearchBar';
import styles from './FilterHierarchy.module.scss';
import FilterHierarchyOptions, {
  FilterHierarchyOption,
} from './FilterHierarchyOptions';
import { OptionLabelsMap } from './utils/getFilterHierarchyLabelsMap';

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

  const selectedValues = useWatch({ name });

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
                    Selected a tier {index + 1} option provides a total value
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
  return mapOptionTreesRecursively(
    0,
    rootOptionIds,
    filterHierarchy,
    optionLabelsMap,
    hierarchySearchTerm,
  )!;
}

function mapOptionTreesRecursively(
  tier: number,
  optionIds: string[],
  filterHierarchy: SubjectMetaFilterHierarchy,
  optionLabelsMap: OptionLabelsMap,
  hierarchySearchTerm: string,
): FilterHierarchyOption[] | undefined {
  if (optionIds.length === 0) {
    return undefined;
  }
  const unfilteredOptionsTree = optionIds.map(optionId => {
    const childOptionIds = filterHierarchy[tier]?.hierarchy[optionId] ?? [];

    return {
      value: optionId,
      label: optionLabelsMap[optionId]?.trim() ?? '',
      filterLabel: optionLabelsMap[filterHierarchy[tier]?.filterId] ?? '',
      childFilterLabel: optionLabelsMap[filterHierarchy[tier]?.childFilterId],
      options: mapOptionTreesRecursively(
        tier + 1,
        childOptionIds,
        filterHierarchy,
        optionLabelsMap,
        hierarchySearchTerm,
      ),
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

  return optionsTree.filter(option => {
    const hasOptions = (option.options?.length ?? 0) > 0;
    const hasSearchTerm = option.label
      .toLowerCase()
      .includes(hierarchySearchTerm.trim().toLowerCase());
    return hasOptions || hasSearchTerm;
  });
}

export default memo(FilterHierarchy);
