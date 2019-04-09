import React from 'react';
import { RadioChangeEventHandler } from 'src/components/form/FormRadio';
import FormRadioGroup from 'src/components/form/FormRadioGroup';

export interface PublicationSubjectMenuOption {
  value: string;
  label: string;
}

interface Props {
  onChange: RadioChangeEventHandler;
  options: PublicationSubjectMenuOption[];
  value: string;
}

const PublicationSubjectMenu = ({ options, onChange, value }: Props) => {
  return (
    <FormRadioGroup
      value={value}
      name="publicationSubject"
      onChange={onChange}
      options={options.map(option => ({
        id: option.value,
        label: option.label,
        value: option.value,
      }))}
      id="publicationSubject"
    />
  );
};

export default PublicationSubjectMenu;
