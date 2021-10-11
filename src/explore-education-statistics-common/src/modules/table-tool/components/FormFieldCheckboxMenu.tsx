import DetailsMenu from '@common/components/DetailsMenu';
import {
  FormFieldCheckboxGroup,
  FormFieldCheckboxSearchGroup,
} from '@common/components/form';
import { FormFieldCheckboxSearchGroupProps } from '@common/components/form/FormFieldCheckboxSearchGroup';
import { useField } from 'formik';
import React, { useEffect, useState } from 'react';
import FormCheckboxSelectionCount from './FormCheckboxSelectedCount';

interface Props<FormValues>
  extends FormFieldCheckboxSearchGroupProps<FormValues> {
  legend: string;
  open?: boolean;
}

function FormFieldCheckboxMenu<FormValues>(props: Props<FormValues>) {
  const { name, open: defaultOpen = false, options, legend } = props;
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
      summary={legend}
      summaryAfter={<FormCheckboxSelectionCount name={name} />}
    >
      {options.length > 1 ? (
        <FormFieldCheckboxSearchGroup<FormValues>
          selectAll
          legendHidden
          {...props}
          name={name}
          options={options}
        />
      ) : (
        <FormFieldCheckboxGroup<FormValues>
          selectAll
          {...props}
          name={name}
          options={options}
        />
      )}
    </DetailsMenu>
  );
}

export default FormFieldCheckboxMenu;
