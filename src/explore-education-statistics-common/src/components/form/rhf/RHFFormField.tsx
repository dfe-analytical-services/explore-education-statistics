import useRegister from '@common/components/form/rhf/hooks/useRegister';
import getErrorMessage from '@common/components/form/rhf/util/getErrorMessage';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormGroup from '@common/components/form/FormGroup';
import React, {
  ChangeEvent,
  ChangeEventHandler,
  ComponentType,
  ReactNode,
  useMemo,
} from 'react';
import {
  FieldValues,
  FieldElement,
  Path,
  useFormContext,
  ChangeHandler,
} from 'react-hook-form';

export type RHFFormFieldComponentProps<Props, TFormValues> =
  FormFieldProps<TFormValues> &
    Omit<Props, 'name' | 'id' | 'value' | 'error'> & {
      id?: string;
      name: Path<TFormValues>;
    };

export interface FormFieldProps<TFormValues> {
  id?: string;
  formGroup?: boolean;
  formGroupClass?: string;
  name: Path<TFormValues>;
  showError?: boolean;
}

interface FormFieldInputProps {
  error?: ReactNode | string;
  id: string;
}

type InternalFormFieldProps<P> = Omit<P, 'id' | 'value' | 'error'> & {
  as?: ComponentType<FormFieldInputProps & P>;
  children?:
    | ((props: {
        id: string;
        field: FieldElement & { error?: string };
      }) => ReactNode)
    | ReactNode;
  id?: string;
  type?: 'checkbox' | 'radio' | 'text' | 'number';
};

export default function RHFFormField<
  Value extends FieldValues,
  Props = Record<string, unknown>,
>({
  as,
  children,
  formGroup = true,
  formGroupClass,
  id: customId,
  name,
  showError = true,
  ...props
}: FormFieldProps<Value> & InternalFormFieldProps<Props>) {
  const {
    formState: { errors },
    register,
  } = useFormContext<Value>();

  const { ref: inputRef, ...field } = useRegister(name, register);
  const { fieldId } = useFormIdContext();
  const id = fieldId(name, customId);

  const error = getErrorMessage(errors, name, showError);

  const component = useMemo(() => {
    const typedProps = props as {
      onBlur?: ChangeHandler;
      onChange?: ChangeEventHandler;
    };
    const Component = as as ComponentType<FormFieldInputProps>;

    if (Component) {
      return (
        <Component
          {...props}
          {...field}
          id={fieldId(name, customId)}
          name={name}
          error={error}
          inputRef={inputRef}
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
            if (!event.defaultPrevented) {
              field.onBlur(event);
            }
          }}
        />
      );
    }

    return typeof children === 'function'
      ? children({
          id,
          field: { ...field, error },
        })
      : children;
  }, [
    as,
    children,
    customId,
    error,
    field,
    fieldId,
    id,
    inputRef,
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
