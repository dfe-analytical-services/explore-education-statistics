import FormCharacterCount from '@common/components/form/FormCharacterCount';
import FormGroup from '@common/components/form/FormGroup';
import FormBaseTextArea, {
  FormTextAreaProps,
} from '@common/components/form/FormBaseTextArea';
import React from 'react';

export default function FormTextArea({
  id,
  maxLength,
  value,
  ...props
}: FormTextAreaProps) {
  if (!!maxLength && maxLength > 0) {
    return (
      <div className="govuk-character-count">
        <FormGroup>
          <FormBaseTextArea
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

  return <FormBaseTextArea {...props} id={id} value={value} />;
}
