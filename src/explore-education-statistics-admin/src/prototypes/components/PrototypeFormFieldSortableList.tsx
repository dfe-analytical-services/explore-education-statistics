import FormSortableList, {
  FormSortableListProps,
} from '@admin/prototypes/components/PrototypeFormSortableList';
import { useFormContext } from '@common/components/form/contexts/FormContext';
import { OmitStrict } from '@common/types/util';
import { Field, FieldProps } from 'formik';
import React from 'react';

type Props<FormValues> = {
  id?: string;
  name: FormValues extends Record<string, unknown> ? keyof FormValues : string;
} & OmitStrict<FormSortableListProps, 'id' | 'value'>;

function FormFieldSortableList<FormValues>(props: Props<FormValues>) {
  const { name, id } = props;

  const { prefixFormId, fieldId } = useFormContext();

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        return (
          <FormSortableList
            {...props}
            id={id ? prefixFormId(id) : fieldId(name as string)}
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
