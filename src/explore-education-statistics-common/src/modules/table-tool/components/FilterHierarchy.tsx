import DetailsMenu from '@common/components/DetailsMenu';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import { SubjectMetaFilterHierarchy } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import get from 'lodash/get';
import React, { memo } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import FilterHierarchyOptions from './FilterHierarchyOptions';

export interface FilterHierarchyProps {
  optionDetailsMap: Dictionary<string>;
  filterHierarchy: SubjectMetaFilterHierarchy;
  id?: string;
  disabled?: boolean;
  name: string;
  open?: boolean;
  onToggle?: (isOpen: boolean) => void;
}

function FilterHierarchy({
  optionDetailsMap,
  filterHierarchy,
  id: customId,
  name,
  disabled,
  open,
  onToggle,
}: FilterHierarchyProps) {
  const tiersTotal = filterHierarchy.length + 1;
  const bottomLevelFilterId = filterHierarchy.at(-1)?.childOptionsFilterId;
  const legend = optionDetailsMap[bottomLevelFilterId ?? ''];

  const selectedValues = useWatch({ name });
  const {
    formState: { errors },
  } = useFormContext();

  const { fieldId } = useFormIdContext();
  const id = fieldId(name);

  return (
    <DetailsMenu
      id={`details-${customId ?? id}`}
      open={open}
      jsRequired
      preventToggle={!!get(errors, name)}
      summary={`${legend}${tiersTotal > 1 ? ` (${tiersTotal} tiers)` : ''}`}
      summaryAfter={<FormCheckboxSelectedCount name={name} />}
      onToggle={onToggle}
    >
      <div id={id}>
        <h4>Browse {legend.toLocaleLowerCase()} by related tiers</h4>
        {Object.keys(filterHierarchy[0].hierarchy).map(optionId => {
          return (
            <FilterHierarchyOptions
              key={optionId}
              optionId={optionId}
              optionDetailsMap={optionDetailsMap}
              filterHierarchy={filterHierarchy}
              name={name}
              disabled={disabled}
              tiersTotal={tiersTotal}
              selectedValues={selectedValues}
              level={0}
            />
          );
        })}
      </div>
    </DetailsMenu>
  );
}

export default memo(FilterHierarchy);
