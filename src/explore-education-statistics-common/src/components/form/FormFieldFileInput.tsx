import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormFileInput, {
  FormFileInputProps,
} from '@common/components/form/FormFileInput';
import useToggle from '@common/hooks/useToggle';
import React from 'react';

type Props<FormValues> = FormFieldComponentProps<
  FormFileInputProps,
  FormValues
>;

const FormFieldFileInput = <FormValues extends {}>(
  props: Props<FormValues>,
) => {
  const [fileHasBeenSelected, toggleFileHasBeenSelected] = useToggle(false);

  return (
    <FormField<File | null> {...props}>
      {({ field, helpers }) => (
        <FormFileInput
          {...props}
          {...field}
          onChange={event => {
            toggleFileHasBeenSelected();

            if (props.onChange) {
              props.onChange(event);
            }

            if (event.isDefaultPrevented()) {
              return;
            }

            const file =
              event.target.files && event.target.files.length > 0
                ? event.target.files[0]
                : null;

            helpers.setValue(file);
          }}
          onBlur={event => {
            if (fileHasBeenSelected) field.onBlur(event);
          }}
        />
      )}
    </FormField>
  );
};

export default FormFieldFileInput;
