import FormTextSearchInput from '@common/components/form/FormTextSearchInput';
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import useMounted from '@common/hooks/useMounted';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormRadioGroup, {
  FormRadioGroupProps,
} from '@common/components/form/FormRadioGroup';
import styles from '@common/components/form/FormFieldRadioSearchGroup.module.scss';
import React, { useState } from 'react';

type Props<FormValues, Value extends string> = FormFieldComponentProps<
  FormRadioGroupProps<Value>,
  FormValues
>;

function FormFieldRadioSearchGroup<FormValues, Value extends string = string>(
  props: Props<FormValues, Value>,
) {
  const { name, options } = props;
  const { isMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedOption, setSelectedOption] = useState('');

  if (!isMounted) {
    return <FormFieldRadioGroup {...props} />;
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
    <FormField<Value> {...props}>
      {({ id, field }) => {
        return (
          <>
            <FormTextSearchInput
              id={`${id}-search`}
              name={`${name}-search`}
              label="Search"
              width={20}
              onChange={event => setSearchTerm(event.target.value)}
              onKeyPress={event => {
                if (event.key === 'Enter') {
                  event.preventDefault();
                }
              }}
            />
            <div aria-live="assertive" className={styles.optionsContainer}>
              <FormRadioGroup
                {...props}
                {...field}
                id={id}
                options={filteredOptions}
                onChange={(event, option) => {
                  setSelectedOption(event.target.value);
                  if (props.onChange) {
                    props.onChange(event, option);
                  }

                  if (!event.isDefaultPrevented()) {
                    field.onChange(event);
                  }
                }}
              />
            </div>
          </>
        );
      }}
    </FormField>
  );
}

export default FormFieldRadioSearchGroup;
