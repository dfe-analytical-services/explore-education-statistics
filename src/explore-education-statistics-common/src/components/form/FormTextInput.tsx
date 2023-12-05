import FormBaseInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import FormCharacterCount from '@common/components/form/FormCharacterCount';
import FormGroup from '@common/components/form/FormGroup';
import React from 'react';

export interface FormTextInputProps extends FormBaseInputProps {
  defaultValue?: string;
  pattern?: string;
  value?: string;
}

export default function FormTextInput({
  id,
  maxLength,
  value,
  ...props
}: FormTextInputProps) {
  if (!!maxLength && maxLength > 0) {
    return (
      <div className="govuk-character-count">
        <FormGroup>
          <FormBaseInput
            {...props}
            id={id}
            maxLength={maxLength}
            value={value}
          />
          <FormCharacterCount id={id} maxLength={maxLength} value={value} />
        </FormGroup>
      </div>
    );
  }
  return <FormBaseInput id={id} {...props} value={value} />;
}
