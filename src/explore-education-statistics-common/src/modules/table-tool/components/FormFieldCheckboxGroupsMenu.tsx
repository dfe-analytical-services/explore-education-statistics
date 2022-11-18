import DetailsMenu from '@common/components/DetailsMenu';
import FormFieldCheckboxSearchSubGroups, {
  FormFieldCheckboxSearchSubGroupsProps,
} from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import { useField } from 'formik';
import React from 'react';
import FormCheckboxSelectionCount from './FormCheckboxSelectedCount';

interface Props<FormValues>
  extends FormFieldCheckboxSearchSubGroupsProps<FormValues> {
  legend: string;
  open?: boolean;
}

function FormFieldCheckboxGroupsMenu<FormValues>(props: Props<FormValues>) {
  const { name, legend, open = false } = props;
  const [, meta] = useField(name);

  return (
    <DetailsMenu
      open={open}
      jsRequired
      preventToggle={!!meta.error && meta.touched}
      summary={legend}
      summaryAfter={<FormCheckboxSelectionCount name={name} />}
    >
      <FormFieldCheckboxSearchSubGroups<FormValues> {...props} legendHidden />
    </DetailsMenu>
  );
}

export default FormFieldCheckboxGroupsMenu;
