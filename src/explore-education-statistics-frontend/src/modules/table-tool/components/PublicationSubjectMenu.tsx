import { RadioChangeEventHandler } from '@common/components/form/FormRadio';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import React from 'react';

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
      legend="Choose publication"
      legendHidden
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
