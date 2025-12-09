import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormTextInput, {
  FormTextInputProps,
} from '@common/components/form/FormTextInput';
import React from 'react';
import { FieldValues, useWatch } from 'react-hook-form';
import FormCharacterCount from '@common/components/form/FormCharacterCount';
import FormGroup from './FormGroup';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormTextInputProps,
  TFormValues
>;

export default function FormFieldTextInput<TFormValues extends FieldValues>({
  maxLength,
  ...props
}: Props<TFormValues>) {
  const watchedValue = useWatch({ name: props.name });

  if (!!maxLength && maxLength > 0) {
    return (
      <div className="govuk-character-count">
        <FormGroup>
          <FormField {...props} maxLength={maxLength} as={FormTextInput} />
          <FormCharacterCount
            id={props.id}
            maxLength={maxLength}
            name={props.name}
            value={watchedValue}
          />
        </FormGroup>
      </div>
    );
  }

  return <FormField {...props} as={FormTextInput} />;
}
