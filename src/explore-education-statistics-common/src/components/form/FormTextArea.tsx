import FormGroup from '@common/components/form/FormGroup';
import FormCharacterCount from '@common/components/form/FormCharacterCount';
import FormBaseTextArea, {
  FormTextAreaProps,
} from '@common/components/form/FormBaseTextArea';
import React from 'react';
import { useWatch } from 'react-hook-form';

export default function FormTextArea({
  id,
  maxLength,
  name,
  ...props
}: FormTextAreaProps) {
  const value = useWatch({ name });

  if (!!maxLength && maxLength > 0) {
    return (
      <div className="govuk-character-count">
        <FormGroup>
          <FormBaseTextArea
            {...props}
            id={id}
            maxLength={maxLength}
            name={name}
          />
        </FormGroup>
        <FormCharacterCount id={id} maxLength={maxLength} value={value} />
      </div>
    );
  }

  return (
    <FormBaseTextArea {...props} id={id} maxLength={maxLength} name={name} />
  );
}
