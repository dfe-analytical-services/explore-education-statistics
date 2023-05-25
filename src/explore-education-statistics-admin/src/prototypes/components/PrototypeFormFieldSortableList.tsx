import FormSortableList, {
  FormSortableListProps,
} from '@admin/prototypes/components/PrototypeFormSortableList';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import { OmitStrict } from '@common/types/util';
import { Field, FieldProps } from 'formik';
import React from 'react';

type Props<FormValues> = {
  id?: string;
  name: FormValues extends Record<string, unknown> ? keyof FormValues : string;
} & OmitStrict<FormSortableListProps, 'id' | 'value'>;

function FormFieldSortableList<FormValues>(props: Props<FormValues>) {
  const { name, id } = props;

  const { fieldId } = useFormIdContext();

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        return (
          <FormSortableList
            {...props}
            id={fieldId(name as string, id)}
            value={field.value}
            onChange={value => {
              form.setFieldValue(name as string, value);
            }}
          />
        );
      }}
    </Field>
  );
}

export default FormFieldSortableList;
