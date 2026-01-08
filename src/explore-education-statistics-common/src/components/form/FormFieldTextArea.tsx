import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormTextArea from '@common/components/form/FormTextArea';
import { FormTextAreaProps } from '@common/components/form/FormBaseTextArea';
import React from 'react';
import { FieldValues, useWatch } from 'react-hook-form';
import FormCharacterCount from '@common/components/form/FormCharacterCount';
import FormGroup from './FormGroup';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormTextAreaProps,
  TFormValues
>;

export default function FormFieldTextArea<TFormValues extends FieldValues>({
  maxLength,
  ...props
}: Props<TFormValues>) {
  const watchedValue = useWatch({ name: props.name });

  if (!!maxLength && maxLength > 0) {
    return (
      <div className="govuk-character-count">
        <FormGroup>
          <FormField {...props} maxLength={maxLength} as={FormTextArea} />
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

  return <FormField {...props} as={FormTextArea} />;
}
