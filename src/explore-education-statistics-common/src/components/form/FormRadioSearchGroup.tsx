import FormTextSearchInput from '@common/components/form/FormTextSearchInput';
import useMounted from '@common/hooks/useMounted';
import FormRadioGroup, {
  FormRadioGroupProps,
} from '@common/components/form/FormRadioGroup';
import styles from '@common/components/form/FormFieldRadioSearchGroup.module.scss';
import React, { useState } from 'react';

export interface FormRadioSearchGroupProps extends FormRadioGroupProps {
  searchLabel?: string;
}

const FormRadioSearchGroup = ({
  searchLabel = 'Search',
  ...props
}: FormRadioSearchGroupProps) => {
  const { id, legend, name, options } = props;
  const { isMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedOption, setSelectedOption] = useState('');

  if (!isMounted) {
    return (
      <FormRadioGroup
        {...props}
        name={name}
        legend={legend}
        id={id}
        options={options}
      />
    );
  }

  let filteredOptions = options;

  if (searchTerm) {
    filteredOptions = options.filter(option => {
      return (
        option.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
        selectedOption.indexOf(option.value) > -1
      );
    });
  }

  return (
    <>
      <FormTextSearchInput
        id={`${id}-search`}
        name={`${name}-search`}
        label={searchLabel}
        width={20}
        onChange={event => setSearchTerm(event.target.value)}
        onKeyPress={event => {
          if (event.key === 'Enter') {
            event.preventDefault();
          }
        }}
      />
      <div aria-live="assertive" className={styles.optionsContainer}>
        {filteredOptions.length > 0 ? (
          <FormRadioGroup
            {...props}
            name={name}
            legend={legend}
            id={id}
            options={filteredOptions}
            onChange={(event, option) => {
              setSelectedOption(event.target.value);
              if (props.onChange) {
                props.onChange(event, option);
              }
            }}
          />
        ) : (
          <p>No results found</p>
        )}
      </div>
    </>
  );
};

export default FormRadioSearchGroup;
