import React from 'react';
import { RadioChangeEventHandler } from 'src/components/form/FormRadio';
import FormRadioGroup from 'src/components/form/FormRadioGroup';

export interface PublicationSubjectMenuOption {
  name: string;
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
        id: option.name,
        label: option.label,
        value: option.name,
      }))}
      id="publicationSubject"
    />
  );
};

export default PublicationSubjectMenu;
