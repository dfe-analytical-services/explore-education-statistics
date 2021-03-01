import { useFormContext } from '@common/components/form/contexts/FormContext';
import { FormGroup } from '@common/components/form/index';
import {
  FieldHelperProps,
  FieldInputProps,
  FieldMetaProps,
  useField,
} from 'formik';
import React, {
  ChangeEvent,
  ChangeEventHandler,
  ComponentType,
  FocusEvent,
  FocusEventHandler,
  ReactNode,
  useMemo,
} from 'react';

export type FormFieldComponentProps<Props, FormValues> = FormFieldProps<
  FormValues
> &
  Omit<Props, 'id' | 'value' | 'error'> & {
    id?: string;
  };

export interface FormFieldProps<FormValues = unknown> {
  id?: string;
  formGroup?: boolean;
  formGroupClass?: string;
  name: FormValues extends Record<string, unknown> ? keyof FormValues : string;
  showError?: boolean;
}

interface FormFieldInputProps<Value> extends FieldInputProps<Value> {
  error?: ReactNode | string;
  id: string;
}

type InternalFormFieldProps<P, Value> = Omit<P, 'id' | 'value' | 'error'> & {
  as?: ComponentType<FormFieldInputProps<Value> & P>;
  children?:
    | ((props: {
        id: string;
        field: FieldInputProps<Value> & { error?: string };
        meta: FieldMetaProps<Value>;
        helpers: FieldHelperProps<Value>;
      }) => ReactNode)
    | ReactNode;
  id?: string;
  type?: 'checkbox' | 'radio' | 'text' | 'number';
};

function FormField<Value, Props = Record<string, unknown>>({
  as,
  children,
  formGroup = true,
  formGroupClass,
  id: customId,
  name,
  showError = true,
  type,
  ...props
}: FormFieldProps & InternalFormFieldProps<Props, Value>) {
  const { prefixFormId, fieldId } = useFormContext();
  const id = customId ? prefixFormId(customId) : fieldId(name);

  const [field, meta, helpers] = useField({
    name,
    type,
  });

  const error = showError && meta.error && meta.touched ? meta.error : '';

  const component = useMemo(() => {
    const typedProps = props as {
      onBlur?: FocusEventHandler;
      onChange?: ChangeEventHandler;
    };
    const Component = as as ComponentType<FormFieldInputProps<Value>>;

    if (Component) {
      return (
        <Component
          {...props}
          {...field}
          id={customId ? prefixFormId(customId) : fieldId(name)}
          name={name}
          error={error}
          onChange={(event: ChangeEvent) => {
            if (typedProps.onChange) {
              typedProps.onChange(event);
            }

            if (!event.isDefaultPrevented()) {
              field.onChange(event);
            }
          }}
          onBlur={(event: FocusEvent) => {
            if (typedProps.onBlur) {
              typedProps.onBlur(event);
            }

            if (!event.isDefaultPrevented()) {
              field.onBlur(event);
            }
          }}
        />
      );
    }

    return typeof children === 'function'
      ? children({
          id,
          field: {
            ...field,
            error,
          },
          helpers,
          meta,
        })
      : children;
  }, [
    as,
    children,
    prefixFormId,
    customId,
    error,
    field,
    fieldId,
    helpers,
    id,
    meta,
    name,
    props,
  ]);

  return formGroup ? (
    <FormGroup hasError={!!error} className={formGroupClass}>
      {component}
    </FormGroup>
  ) : (
    component
  );
}

export default FormField;
