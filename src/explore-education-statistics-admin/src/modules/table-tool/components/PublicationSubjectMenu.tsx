import React from 'react';
import {RadioChangeEventHandler} from "../../../components/form/FormRadio";
import FormRadioGroup from "../../../components/form/FormRadioGroup";

interface Props {
  onChange: RadioChangeEventHandler;
  options: {
    id: string;
    name: string;
  }[];
  value: string;
}

const PublicationSubjectMenu = ({ options, onChange, value }: Props) => {
  return (
    <FormRadioGroup
      value={value}
      name="publicationSubjectId"
      onChange={onChange}
      options={options.map(option => ({
        id: option.id,
        label: option.name,
        value: option.id,
      }))}
      id="publicationSubjectId"
    />
  );
};

export default PublicationSubjectMenu;
