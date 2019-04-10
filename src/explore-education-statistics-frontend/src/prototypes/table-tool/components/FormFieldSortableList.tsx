import { Field, FieldProps } from 'formik';
import React from 'react';
import FormSortableList, {
  FormSortableListProps,
} from 'src/prototypes/table-tool/components/FormSortableList';
import { Omit } from 'src/types/util';

type Props<FormValues> = {
  name: keyof FormValues | string;
} & Omit<FormSortableListProps, 'value'>;

const FormFieldSortableList = <T extends {}>(props: Props<T>) => {
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
