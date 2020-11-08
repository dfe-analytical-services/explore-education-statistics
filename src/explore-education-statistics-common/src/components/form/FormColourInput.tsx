import BaseFormInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import React from 'react';

export interface FormColourInputProps extends FormBaseInputProps {
  colours?: string[];
  value?: string;
}

const FormColourInput = ({
  colours = [],
  id,
  ...props
}: FormColourInputProps) => {
  return (
    <>
      <BaseFormInput {...props} id={id} type="color" list={`${id}-colours`} />

      {colours.length > 0 && (
        <datalist id={`${id}-colours`}>
          {colours.map((colour, index) => (
            // eslint-disable-next-line react/no-array-index-key
            <option key={index} value={colour} />
          ))}
        </datalist>
      )}
    </>
  );
};

export default FormColourInput;
