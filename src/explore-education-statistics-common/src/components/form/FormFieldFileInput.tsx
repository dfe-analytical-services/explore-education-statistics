/* eslint-disable react/destructuring-assignment */
import Effect from '@common/components/Effect';
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

function FormFieldFileInput<FormValues>(props: Props<FormValues>) {
  // Have to do additional tracking as file inputs have
  // weird quirks that mean we get a weird validation
  // experience due to:
  // 1. `onBlur` triggering immediately upon clicking
  //     (should be after selecting file or cancelling)
  // 2. `onChange` not triggering if nothing is selected
  const [isClicked, toggleClicked] = useToggle(false);
  // undefined is the initial state, null is unselected state
  const [inputValue, setInputValue] = useState<File | null | undefined>();

  return (
    <FormField<File | null> {...props}>
      {({ id, field, helpers, meta }) => (
        <>
          <Effect
            value={meta.touched}
            onChange={(touched, previousTouched) => {
              // Form has been reset
              if (previousTouched && !touched) {
                toggleClicked.off();
                setInputValue(undefined);
              }
            }}
          />

          <FormFileInput
            {...props}
            {...field}
            id={id}
            onClick={toggleClicked.on}
            onChange={event => {
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
              if (isClicked && typeof inputValue === 'undefined') {
                toggleClicked.off();
                return;
              }

              field.onBlur(event);
            }}
          />
        </>
      )}
    </FormField>
  );
}

export default FormFieldFileInput;
