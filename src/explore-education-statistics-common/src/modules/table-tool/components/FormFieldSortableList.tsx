import { OmitStrict } from '@common/types/util';
import { Field, FieldProps } from 'formik';
import React from 'react';
import FormSortableList, { FormSortableListProps } from './FormSortableList';

type Props<FormValues> = {
  name: FormValues extends {} ? keyof FormValues : string;
} & OmitStrict<FormSortableListProps, 'value'>;

const FormFieldSortableList = <FormValues extends {}>(
  props: Props<FormValues>,
) => {
  const { name } = props;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        return (
          <FormSortableList
            {...props}
            {...field}
            onChange={value => {
              form.setFieldValue(name as string, value);
            }}
          />
        );
      }}
    </Field>
  );
};

export default FormFieldSortableList;
