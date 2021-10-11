import DetailsMenu from '@common/components/DetailsMenu';
import FormFieldCheckboxSearchSubGroups, {
  FormFieldCheckboxSearchSubGroupsProps,
} from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import { useField } from 'formik';
import React, { useEffect, useState } from 'react';
import FormCheckboxSelectionCount from './FormCheckboxSelectedCount';

interface Props<FormValues>
  extends FormFieldCheckboxSearchSubGroupsProps<FormValues> {
  legend: string;
  open?: boolean;
}

function FormFieldCheckboxGroupsMenu<FormValues>(props: Props<FormValues>) {
  const { name, legend, open: defaultOpen = false } = props;
  const [open, setOpen] = useState(defaultOpen);

  const [, meta] = useField(name);

  useEffect(() => {
    if (meta.error && meta.touched) {
      setOpen(true);
    }
  }, [meta.error, meta.touched]);

  return (
    <DetailsMenu
      open={open}
      jsRequired
      onToggle={(isOpen, event) => {
        if (meta.error && meta.touched) {
          event.preventDefault();
        }
      }}
      summary={legend}
      summaryAfter={<FormCheckboxSelectionCount name={name} />}
    >
      <FormFieldCheckboxSearchSubGroups<FormValues> {...props} legendHidden />
    </DetailsMenu>
  );
}

export default FormFieldCheckboxGroupsMenu;
