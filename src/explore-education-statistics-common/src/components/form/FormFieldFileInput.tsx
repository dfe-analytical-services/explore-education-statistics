import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormFileInput, {
  FormFileInputProps,
} from '@common/components/form/FormFileInput';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';

type Props<FormValues> = FormFieldComponentProps<
  FormFileInputProps,
  FormValues
>;

const FormFieldFileInput = <FormValues extends {}>(
  props: Props<FormValues>,
) => {
  const [fileHasBeenSelected, toggleFileHasBeenSelected] = useToggle(false);
  const [inputValue, setInputValue] = useState<File | null>(null);

  return (
    <FormField<File | null> {...props}>
      {({ field, helpers }) => {
        return (
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

              setInputValue(file);
              helpers.setValue(file);
            }}
            onBlur={event => {
              if (inputValue !== field.value) {
                // formField value was outside of the input (e.g form reset)
                // reset field touched state
                helpers.setTouched(false);
                toggleFileHasBeenSelected(false);
              } else if (fileHasBeenSelected) {
                // only allow field validation if a file has been previously selected
                field.onBlur(event);
              }
            }}
          />
        );
      }}
    </FormField>
  );
};

export default FormFieldFileInput;
