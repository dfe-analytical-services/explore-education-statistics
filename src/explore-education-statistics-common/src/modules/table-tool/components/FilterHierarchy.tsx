import DetailsMenu from '@common/components/DetailsMenu';
import { FormFieldset } from '@common/components/form';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import { SubjectMetaFilterHierarchy } from '@common/services/tableBuilderService';
import get from 'lodash/get';
import React, { memo } from 'react';
import { useFormContext, useWatch } from 'react-hook-form';
import FilterHierarchyOptions from './FilterHierarchyOptions';
import { OptionLabelsMap } from './utils/getFilterHierarchyOptionLabelsMap';

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
  const bottomLevelFilterId = filterHierarchy.at(-1)?.childFilterId;
  const legend = optionLabelsMap[bottomLevelFilterId ?? ''];

  const selectedValues = useWatch({ name });
  const {
    formState: { errors },
  } = useFormContext();

  const { fieldId } = useFormIdContext();
  const id = fieldId(name);
  const errorMessage = get(errors, name)?.message;

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
      <div id={id}>
        <FormFieldset
          id={name}
          legend={`Browse ${legend.toLocaleLowerCase()} by related tiers`}
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
                name={name}
                disabled={disabled}
                tiersTotal={tiersTotal}
                selectedValues={selectedValues}
                level={0}
              />
            );
          })}
        </FormFieldset>
      </div>
    </DetailsMenu>
  );
}

export default memo(FilterHierarchy);
