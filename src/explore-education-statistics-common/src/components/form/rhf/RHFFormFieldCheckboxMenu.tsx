import DetailsMenu from '@common/components/DetailsMenu';
import { FormFieldCheckboxSearchGroupProps } from '@common/components/form/FormFieldCheckboxSearchGroup';
import RHFFormCheckboxSelectedCount from '@common/components/form/rhf/RHFFormCheckboxSelectedCount';
import RHFFormFieldCheckboxSearchGroup from '@common/components/form/rhf/RHFFormFieldCheckboxSearchGroup';
import RHFFormFieldCheckboxGroup from '@common/components/form/rhf/RHFFormFieldCheckboxGroup';
import get from 'lodash/get';
import React, { useEffect, useState } from 'react';
import { FieldValues, useFormContext } from 'react-hook-form';

interface Props<TFormValues extends FieldValues>
  extends FormFieldCheckboxSearchGroupProps<TFormValues> {
  legend: string;
  open?: boolean;
}

export default function RHFFormFieldCheckboxMenu<
  TFormValues extends FieldValues,
>(props: Props<TFormValues>) {
  const { name, open: defaultOpen = false, options, legend } = props;
  const [open, setOpen] = useState(defaultOpen);

  const {
    formState: { errors },
  } = useFormContext();

  useEffect(() => {
    if (!open && get(errors, name)) {
      setOpen(true);
    }
  }, [errors, name, open, setOpen]);

  return (
    <DetailsMenu
      open={open}
      jsRequired
      summary={legend}
      summaryAfter={<RHFFormCheckboxSelectedCount name={name} />}
    >
      {options.length > 1 ? (
        <RHFFormFieldCheckboxSearchGroup
          selectAll
          legendHidden
          {...props}
          name={name}
          options={options}
        />
      ) : (
        <RHFFormFieldCheckboxGroup {...props} name={name} options={options} />
      )}
    </DetailsMenu>
  );
}
