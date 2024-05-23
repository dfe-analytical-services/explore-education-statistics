import FormRadioGroup, {
  FormRadioGroupProps,
} from '@common/components/form/FormRadioGroup';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import { RadioChangeEventHandler } from '@common/components/form/FormRadio';
import getErrorMessage from '@common/components/form/util/getErrorMessage';
import React, { memo, useCallback } from 'react';
import { FieldValues, Path, useFormContext, useWatch } from 'react-hook-form';

export interface FormFieldRadioGroupProps<TFormValues extends FieldValues>
  extends Omit<
    FormRadioGroupProps,
    'name' | 'value' | 'inputRef' | 'id' | 'error'
  > {
  name: Path<TFormValues>;
  id?: string;
  showError?: boolean;
}

function FormFieldRadioGroup<TFormValues extends FieldValues>({
  name,
  id: customId,
  showError = true,
  ...props
}: FormFieldRadioGroupProps<TFormValues>) {
  const {
    formState: { errors },
    register,
  } = useFormContext<TFormValues>();

  // Use standard register instead of `useRegister` as the memoisation
  // there causes the field to not work after reseting the form.
  const { ref: inputRef, ...field } = register(name);
  const { fieldId } = useFormIdContext();
  const id = fieldId(name, customId);

  const selectedValue = useWatch({ name }) || '';
  const propsOnChange = props.onChange;
  const fieldOnChange = field.onChange;

  const handleChange: RadioChangeEventHandler = useCallback(
    async (event, option) => {
      if (propsOnChange) {
        propsOnChange(event, option);
      }

      if (event.isDefaultPrevented()) {
        return;
      }

      await fieldOnChange(event);
    },
    [propsOnChange, fieldOnChange],
  );

  return (
    <FormRadioGroup
      {...props}
      {...field}
      error={getErrorMessage(errors, name, showError)}
      id={id}
      inputRef={inputRef}
      value={selectedValue}
      onChange={handleChange}
    />
  );
}

export default memo(FormFieldRadioGroup) as typeof FormFieldRadioGroup;
