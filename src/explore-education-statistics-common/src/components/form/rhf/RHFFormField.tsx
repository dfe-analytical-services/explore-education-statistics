import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormGroup from '@common/components/form/FormGroup';
import useRegister from '@common/components/form/rhf/hooks/useRegister';
import { CheckboxChangeEventHandler } from '@common/components/form/FormCheckbox';
import getErrorMessage from '@common/components/form/rhf/util/getErrorMessage';
import { mergeRefs } from '@common/utils/mergeRefs';
import React, {
  ChangeEvent,
  ChangeEventHandler,
  ComponentType,
  FocusEventHandler,
  ReactNode,
  Ref,
  useMemo,
} from 'react';
import { FieldValues, Path, useFormContext } from 'react-hook-form';

export type FormFieldComponentProps<
  TProps extends FormFieldInputProps,
  TFormValues,
> = FormFieldProps<TFormValues> &
  Omit<TProps, 'name' | 'id' | 'value' | 'error'> & {
    id?: string;
    name: Path<TFormValues>;
  };

interface FormFieldInputProps {
  error?: ReactNode | string;
  id: string;
  inputRef?: Ref<Element>;
  name: string;
  onChange?: ChangeEventHandler | CheckboxChangeEventHandler;
  onBlur?: FocusEventHandler;
}

export type FormFieldProps<TFormValues> = {
  children?: ReactNode;
  id?: string;
  formGroup?: boolean;
  formGroupClass?: string;
  inputRef?: Ref<Element>;
  name: Path<TFormValues>;
  showError?: boolean;
  onChange?: ChangeEventHandler;
  onBlur?: FocusEventHandler;
};

type InternalFormFieldProps<TProps> = Omit<TProps, 'id' | 'value' | 'error'> & {
  as?: ComponentType<FormFieldInputProps & TProps>;
};

export default function RHFFormField<
  TFormValues extends FieldValues,
  TProps extends FormFieldInputProps,
>({
  as,
  children,
  formGroup = true,
  formGroupClass,
  id: customId,
  inputRef,
  name,
  showError = true,
  onBlur,
  onChange,
  ...props
}: FormFieldProps<TFormValues> & InternalFormFieldProps<TProps>) {
  const {
    formState: { errors },
    register,
  } = useFormContext<TFormValues>();

  const {
    ref: fieldRef,
    onBlur: fieldOnBlur,
    onChange: fieldOnChange,
  } = useRegister(name, register);

  const { fieldId } = useFormIdContext();

  const error = getErrorMessage(errors, name, showError);

  const component = useMemo(() => {
    const Component = as as ComponentType<FormFieldInputProps>;
    if (Component) {
      return (
        <Component
          {...props}
          error={error}
          id={fieldId(name, customId)}
          name={name}
          inputRef={mergeRefs(fieldRef, inputRef)}
          onChange={async (event: ChangeEvent) => {
            onChange?.(event);

            if (!event.defaultPrevented) {
              await fieldOnChange(event);
            }
          }}
          onBlur={async event => {
            onBlur?.(event);

            if (!event.isDefaultPrevented()) {
              await fieldOnBlur(event);
            }
          }}
        />
      );
    }

    return children;
  }, [
    as,
    children,
    props,
    error,
    fieldId,
    name,
    customId,
    fieldRef,
    inputRef,
    onChange,
    fieldOnChange,
    onBlur,
    fieldOnBlur,
  ]);

  return formGroup ? (
    <FormGroup hasError={!!error} className={formGroupClass}>
      {component}
    </FormGroup>
  ) : (
    component
  );
}
