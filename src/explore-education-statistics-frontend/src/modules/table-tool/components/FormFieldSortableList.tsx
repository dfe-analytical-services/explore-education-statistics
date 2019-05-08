import { Omit } from '@common/types/util';
import FormSortableList, {
  FormSortableListProps,
} from '@frontend/modules/table-tool/components/FormSortableList';
import { Field, FieldProps } from 'formik';
import React from 'react';

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
