import useRegister from '@common/components/form/hooks/useRegister';
import { useFormContext, useWatch } from 'react-hook-form';
import React, { memo } from 'react';
import FilterHierarchyOptions from './FilterHierarchyOptions';

export type FilterHierarchyFormFieldOptionsTree = {
  value: string;
  label: string;
  options?: FilterHierarchyFormFieldOptionsTree;
}[];

interface FilterHierarchyFormFieldOptionsProps {
  disabled?: boolean;
  level?: number;
  name: string;
  optionsTree?: FilterHierarchyFormFieldOptionsTree;
  totalColumns: number;
}

function FilterHierarchyFormFieldOptions({
  name,
  optionsTree,
  ...props
}: FilterHierarchyFormFieldOptionsProps) {
  const { register } = useFormContext();
  const { ref: inputRef, ...field } = useRegister(name, register);

  const value = useWatch({ name }) || [];

  if (!optionsTree) {
    return undefined;
  }

  return (
    <>
      <FilterHierarchyOptions
        {...props}
        {...field}
        id={name}
        value={value}
        optionsTree={optionsTree}
      />
    </>
  );
}

export default memo(FilterHierarchyFormFieldOptions);
