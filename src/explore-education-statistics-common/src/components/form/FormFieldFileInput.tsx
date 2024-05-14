import Effect from '@common/components/Effect';
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormFileInput, {
  FormFileInputProps,
} from '@common/components/form/FormFileInput';
import useRegister from '@common/components/form/hooks/useRegister';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';
import { FieldValues, Path, PathValue, useFormContext } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormFileInputProps,
  TFormValues
>;

export default function FormFieldFileInput<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  // Have to do additional tracking as file inputs have
  // weird quirks that mean we get a weird validation
  // experience due to:
  // 1. `onBlur` triggering immediately upon clicking
  //     (should be after selecting file or cancelling)
  // 2. `onChange` not triggering if nothing is selected
  const [isClicked, toggleClicked] = useToggle(false);
  // undefined is the initial state, null is unselected state
  const [inputValue, setInputValue] = useState<File | null | undefined>();

  const { name } = props;

  const { getFieldState, register, setValue } = useFormContext<TFormValues>();

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { ref, ...field } = useRegister(name, register);
  const { fieldId } = useFormIdContext();
  const id = fieldId(name);

  const { onChange } = props;

  const fieldState = getFieldState(name);

  return (
    <FormField {...props} name={name}>
      <>
        <Effect
          value={fieldState.isTouched}
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
          error={fieldState.error?.message}
          id={id}
          onClick={toggleClicked.on}
          onChange={event => {
            onChange?.(event);

            if (event.isDefaultPrevented()) {
              return;
            }

            const file =
              event.target.files && event.target.files.length > 0
                ? event.target.files[0]
                : null;

            if (file) {
              setInputValue(file);
              setValue(
                name,
                file as PathValue<TFormValues, Path<TFormValues>>,
                {
                  shouldTouch: true,
                },
              );
            }
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
    </FormField>
  );
}
