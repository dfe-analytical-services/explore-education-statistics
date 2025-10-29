import FormFieldCheckboxSearchSubGroups, {
  FormFieldCheckboxSearchSubGroupsProps,
} from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import FilterAccordion from '@common/modules/table-tool/components/FilterAccordion';
import { OmitStrict } from '@common/types';
import React, { useEffect, useState } from 'react';
import { FieldValues, useFormContext } from 'react-hook-form';
import get from 'lodash/get';

interface Props<TFormValues extends FieldValues>
  extends OmitStrict<
    FormFieldCheckboxSearchSubGroupsProps<TFormValues>,
    'legendHidden'
  > {
  hiddenText?: string;
  legend: string;
  open?: boolean;
  onToggle?: (isOpen: boolean) => void;
}

export default function FormFieldCheckboxGroupsMenu<
  TFormValues extends FieldValues,
>(props: Props<TFormValues>) {
  const {
    hiddenText,
    legend,
    name,
    open: defaultOpen = false,
    id,
    onToggle,
  } = props;
  const [open, setOpen] = useState(defaultOpen);

  const {
    formState: { errors },
  } = useFormContext();

  // Groups with an error are opened, so add them to the list of open
  // filters to prevent the group collapsing as soon as you select
  // an option in the group.
  useEffect(() => {
    if (!open && get(errors, name)) {
      setOpen(true);
      onToggle?.(true);
    }
  }, [errors, name, open, onToggle]);

  useEffect(() => {
    setOpen(defaultOpen);
  }, [defaultOpen]);

  return (
    <FilterAccordion
      id={`${id}-options`}
      open={open}
      label={legend}
      labelAfter={<FormCheckboxSelectedCount name={name} />}
      labelHiddenText={hiddenText}
      preventToggle={!!get(errors, name)}
      testId={`${id}-accordion`}
      onToggle={onToggle}
    >
      <FormFieldCheckboxSearchSubGroups {...props} legendHidden />
    </FilterAccordion>
  );
}
