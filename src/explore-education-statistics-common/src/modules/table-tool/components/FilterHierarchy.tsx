import DetailsMenu from '@common/components/DetailsMenu';
import { FormFieldset } from '@common/components/form';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import { SubjectMetaFilterHierarchy } from '@common/services/tableBuilderService';
import get from 'lodash/get';
import React, { memo, useCallback, useState } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import FilterHierarchyOptions from './FilterHierarchyOptions';
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
          {Object.keys(filterHierarchy[0].hierarchy).map(optionId => {
            return (
              <FilterHierarchyOptions
                key={optionId}
                optionId={optionId}
                optionLabelsMap={optionLabelsMap}
                filterHierarchy={filterHierarchy}
                filterLabels={filterLabels}
                name={name}
                disabled={disabled}
                tiersTotal={tiersTotal}
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
