import { FormGroup } from '@common/components/form/index';
import createErrorHelper from '@common/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React, {
  ChangeEventHandler,
  ComponentType,
  FocusEventHandler,
  ReactNode,
} from 'react';

interface RequiredProps<FormValues> {
  name: keyof FormValues | string;
  error?: ReactNode | string;
  value: unknown;
  onChange?: ChangeEventHandler;
  onBlur?: FocusEventHandler;
}

export type Props<FormValues, P> = {
  formGroupClass?: string;
  error?: ReactNode | string;
  name: keyof FormValues | string;
  type?: 'checkbox' | 'radio' | 'text' | 'number';
  validate?: (value: unknown) => string | Promise<void> | undefined;
  as?: ComponentType<P & RequiredProps<FormValues>>;
  showError?: boolean;
} & Omit<P, 'name' | 'value' | 'type'>;

const FormField = <P extends {}, FormValues>({
  as,
  formGroupClass,
  error,
  name,
  showError,
  type,
  ...props
}: Props<FormValues, P>) => {
  const Component = as as ComponentType<RequiredProps<FormValues>>;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        let errorMessage = error || getError(name);

        if (!showError) {
          errorMessage = '';
        }

        const typeProps: { checked?: boolean } = {};

        switch (type) {
          case 'checkbox':
          case 'radio':
            typeProps.checked = !!field.value;
            break;
          default:
            break;
        }

        return (
          <FormGroup hasError={!!errorMessage} className={formGroupClass}>
            <Component
              {...props}
              {...field}
              {...typeProps}
              name={name}
              error={errorMessage}
              onChange={event => {
                const typedProps = props as { onChange?: ChangeEventHandler };

                if (typedProps.onChange) {
                  typedProps.onChange(event);
                }

                if (!event.isDefaultPrevented()) {
                  field.onChange(event);
                }
              }}
              onBlur={event => {
                const typedProps = props as { onBlur?: FocusEventHandler };

                if (typedProps.onBlur) {
                  typedProps.onBlur(event);
                }

                if (!event.isDefaultPrevented()) {
                  field.onBlur(event);
                }
              }}
            />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormField;
