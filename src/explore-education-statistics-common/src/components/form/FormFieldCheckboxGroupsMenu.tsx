import DetailsMenu from '@common/components/DetailsMenu';
import FormFieldCheckboxSearchSubGroups, {
  FormFieldCheckboxSearchSubGroupsProps,
} from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import { OmitStrict } from '@common/types';
import React, { useEffect } from 'react';
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
  const { hiddenText, legend, name, open = false, id, onToggle } = props;

  const {
    formState: { errors },
  } = useFormContext();

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
      id={`details-${id}`}
      open={open}
      hiddenText={hiddenText}
      jsRequired
      preventToggle={!!get(errors, name)}
      summary={legend}
      summaryAfter={<FormCheckboxSelectedCount name={name} />}
      onToggle={onToggle}
    >
      <FormFieldCheckboxSearchSubGroups {...props} legendHidden />
    </DetailsMenu>
  );
}
