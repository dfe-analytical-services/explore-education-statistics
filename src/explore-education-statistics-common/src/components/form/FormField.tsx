import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React, {
  ChangeEventHandler,
  ComponentType,
  FocusEventHandler,
} from 'react';

interface RequiredProps<FormValues> {
  name: keyof FormValues | string;
  error?: string;
  value: unknown;
  onChange?: ChangeEventHandler;
  onBlur?: FocusEventHandler;
}

type Props<FormValues, P> = {
  name: keyof FormValues | string;
  type?: string;
  validate?: (value: unknown) => string | Promise<void> | undefined;
  as?: ComponentType<P & RequiredProps<FormValues>>;
} & Omit<P, 'name' | 'value' | 'type'>;

const FormField = <P extends {}, FormValues>({
  name,
  as,
  type,
  ...props
}: Props<FormValues, P>) => {
  const Component = as as ComponentType<RequiredProps<FormValues>>;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        const error = getError(name);

        const typeProps: { checked?: boolean } = {};

        if (type === 'checkbox') {
          typeProps.checked = !!field.value;
        } else if (type === 'radio') {
          typeProps.checked = !!field.value;
        }

        return (
          <Component
            {...props}
            {...field}
            {...typeProps}
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
            name={name}
            error={error}
          />
        );
      }}
    </Field>
  );
};

export default FormField;
