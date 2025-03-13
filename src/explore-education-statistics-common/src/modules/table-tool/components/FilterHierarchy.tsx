import DetailsMenu from '@common/components/DetailsMenu';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import get from 'lodash/get';
import React, { memo } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import FilterHierarchyOptions, {
  FilterHierarchyOptionsTree,
} from './FilterHierarchyOptions';

export interface FilterHierarchyType<T = FilterHierarchyOptionsTree> {
  legend: string;
  options: T;
  id: string;
  levelsTotal: number;
}

interface Props extends FilterHierarchyType {
  disabled?: boolean;
  name: string;
  open?: boolean;
  onToggle?: (isOpen: boolean) => void;
}

function FilterHierarchy({
  disabled,
  legend,
  options,
  id: customId,
  name,
  open = false,
  levelsTotal,
  onToggle,
}: Props) {
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
      summary={`${legend}${levelsTotal > 1 ? ` (${levelsTotal} tiers)` : ''}`}
      summaryAfter={<FormCheckboxSelectedCount name={name} />}
      onToggle={onToggle}
    >
      <div id={id}>
        <span>
          field-name - {name}: {JSON.stringify(selectedValues)}
        </span>

        <h4>Browse qualifications by related tiers</h4>

        <FilterHierarchyOptions
          value={selectedValues}
          disabled={disabled}
          name={name}
          optionsTree={options}
          totalColumns={levelsTotal}
        />
      </div>
    </DetailsMenu>
  );
}

export default memo(FilterHierarchy);
