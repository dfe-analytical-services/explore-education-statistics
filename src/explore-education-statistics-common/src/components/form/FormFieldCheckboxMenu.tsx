import FormCheckboxSelectedCount from '@common/components/form/FormCheckboxSelectedCount';
import FormFieldCheckboxSearchGroup from '@common/components/form/FormFieldCheckboxSearchGroup';
import FormFieldCheckboxGroup from '@common/components/form/FormFieldCheckboxGroup';
import { FormFieldComponentProps } from '@common/components/form/FormField';
import { FormCheckboxSearchGroupProps } from '@common/components/form/FormCheckboxSearchGroup';
import FilterAccordion from '@common/modules/table-tool/components/FilterAccordion';
import { OmitStrict } from '@common/types';
import get from 'lodash/get';
import React, { useEffect, useState } from 'react';
import { FieldValues, useFormContext } from 'react-hook-form';

export type FormFieldCheckboxSearchGroupProps<FormValues> = OmitStrict<
  FormFieldComponentProps<FormCheckboxSearchGroupProps, FormValues>,
  'formGroup'
>;

interface Props<TFormValues extends FieldValues>
  extends FormFieldCheckboxSearchGroupProps<TFormValues> {
  legend: string;
  open?: boolean;
}

export default function FormFieldCheckboxMenu<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  const { name, open: defaultOpen = false, options, legend, id } = props;
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
    <FilterAccordion
      id={`${id}-options`}
      open={open}
      label={legend}
      labelAfter={<FormCheckboxSelectedCount name={name} />}
      testId={`${id}-accordion`}
    >
      {options.length > 1 ? (
        <FormFieldCheckboxSearchGroup
          selectAll
          legendHidden
          {...props}
          name={name}
          options={options}
        />
      ) : (
        <FormFieldCheckboxGroup {...props} name={name} options={options} />
      )}
    </FilterAccordion>
  );
}
