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
import React, { memo, useCallback, useMemo, useState } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
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
      <FormFieldset
        id={name}
        legend={`Search all tiers and ${legend.toLowerCase()}`}
        legendSize="s"
        useFormId
        error={errorMessage?.toString()}
      >
        <Modal
          title={`${legend} tiers`}
          triggerButton={
            <ButtonText>
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
              >
                <h3 className="govuk-heading-s">
                  {filterLabel} (tier {index + 1})
                </h3>
                {isFirst && !isLast && (
                  <>
                    <p>This is a top level category.</p>
                    <p>
                      Selecting a tier 1 option provides a total value for all
                      sub categories associated to this area
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
    </DetailsMenu>
  );
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

export default memo(FilterHierarchy);
