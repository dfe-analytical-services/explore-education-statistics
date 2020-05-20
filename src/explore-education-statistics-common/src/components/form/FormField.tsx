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

export type FormFieldComponentProps<Props> = FormFieldProps &
  Omit<Props, 'value' | 'error'>;

export interface FormFieldProps {
  formGroup?: boolean;
  formGroupClass?: string;
  name: string;
  showError?: boolean;
}

interface FormFieldInputProps<Value> extends FieldInputProps<Value> {
  error?: ReactNode | string;
}

type InternalFormFieldProps<P, Value> = Omit<P, 'value' | 'error'> & {
  as?: ComponentType<FormFieldInputProps<Value> & P>;
  children?:
    | ((props: {
        field: FieldInputProps<Value> & { error?: string };
        meta: FieldMetaProps<Value>;
        helpers: FieldHelperProps<Value>;
      }) => ReactNode)
    | ReactNode;
  type?: 'checkbox' | 'radio' | 'text' | 'number';
};

const FormField = <Value, Props extends {} = {}>({
  as,
  children,
  formGroup = true,
  formGroupClass,
  name,
  showError = true,
  type,
  ...props
}: FormFieldProps & InternalFormFieldProps<Props, Value>) => {
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
          field: {
            ...field,
            error,
          },
          helpers,
          meta,
        })
      : children;
  }, [as, children, error, field, helpers, meta, name, props]);

  return formGroup ? (
    <FormGroup hasError={!!error} className={formGroupClass}>
      {component}
    </FormGroup>
  ) : (
    component
  );
};

export default FormField;
