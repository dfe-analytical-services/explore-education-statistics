import DetailsMenu from '@common/components/DetailsMenu';
import FormFieldCheckboxSearchSubGroups, {
  FormFieldCheckboxSearchSubGroupsProps,
} from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import { useField } from 'formik';
import React, { useEffect, useState } from 'react';
import FormCheckboxSelectionCount from './FormCheckboxSelectedCount';

const FormFieldCheckboxGroupsMenu = <FormValues extends {}>(
  props: FormFieldCheckboxSearchSubGroupsProps<FormValues>,
) => {
  const { name, legend } = props;
  const [open, setOpen] = useState(false);

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
      summary={
        <>
          {legend}
          <FormCheckboxSelectionCount name={name} />
        </>
      }
    >
      <FormFieldCheckboxSearchSubGroups<FormValues> {...props} legendHidden />
    </DetailsMenu>
  );
};

export default FormFieldCheckboxGroupsMenu;
