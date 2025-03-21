import DetailsMenu from '@common/components/DetailsMenu';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import get from 'lodash/get';
import React, { memo } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import FilterHierarchyOptions, {
  FilterHierarchyOption,
} from './FilterHierarchyOptions';

export interface FilterHierarchyType {
  legend: string;
  options?: FilterHierarchyOption[];
  id: string;
  tiersTotal: number;
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
  tiersTotal,
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
      summary={`${legend}${tiersTotal > 1 ? ` (${tiersTotal} tiers)` : ''}`}
      summaryAfter={<FormCheckboxSelectedCount name={name} />}
      onToggle={onToggle}
    >
      <div id={id}>
        <h4>Browse {legend.toLocaleLowerCase()} by related tiers</h4>

        <FilterHierarchyOptions
          value={selectedValues}
          disabled={disabled}
          name={name}
          options={options}
          totalColumns={tiersTotal}
        />
      </div>
    </DetailsMenu>
  );
}

export default memo(FilterHierarchy);
