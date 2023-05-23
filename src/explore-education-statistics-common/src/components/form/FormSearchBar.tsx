import FormBaseInput from '@common/components/form/FormBaseInput';
import FormGroup from '@common/components/form/FormGroup';
import SearchIcon from '@common/components/SearchIcon';
import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@common/components/form/FormSearchBar.module.scss';
import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect, useState } from 'react';

interface Props {
  className?: string;
  hideLabel?: boolean;
  id: string;
  label: string;
  labelSize?: 'xl' | 'l' | 'm' | 's';
  min?: number;
  name: string;
  value?: string;
  onChange?: (value: string) => void;
  onReset?: () => void;
  onSubmit?: (value: string) => void;
}

const FormSearchBar = ({
  className,
  hideLabel,
  id,
  label,
  labelSize = 'm',
  min = 3,
  name,
  value: initialValue = '',
  onChange,
  onReset,
  onSubmit,
}: Props) => {
  const [searchTerm, setSearchTerm] = useState<string>(initialValue);
  const [hasSubmitted, toggleHasSubmitted] = useToggle(false);
  const [showError, toggleError] = useToggle(false);

  useEffect(() => {
    setSearchTerm(initialValue);
  }, [initialValue]);

  const handleSubmit = () => {
    if (searchTerm.length >= min) {
      toggleHasSubmitted.on();
      toggleError.off();
      onSubmit?.(searchTerm);
    } else {
      toggleError.on();
    }
  };

  return (
    <FormGroup className={className} hasError={showError}>
      <FormBaseInput
        addOn={
          <button
            className={styles.button}
            type={onSubmit ? 'button' : 'submit'}
            onClick={handleSubmit}
          >
            <SearchIcon className={styles.icon} />
            <VisuallyHidden>Search</VisuallyHidden>
          </button>
        }
        className={styles.input}
        error={
          showError ? `Search must be at least ${min} characters` : undefined
        }
        hideLabel={hideLabel}
        id={id}
        label={label}
        labelSize={labelSize}
        name={name}
        type="search"
        value={searchTerm}
        onChange={event => {
          setSearchTerm(event.target.value);
          onChange?.(event.target.value);
          if (event.target.value.length >= min || !event.target.value.length) {
            toggleError.off();
          }
        }}
        onKeyPress={event => {
          if (onSubmit && event.key === 'Enter') {
            event.preventDefault();
            handleSubmit();
          }
        }}
      />
      {onReset && hasSubmitted && (
        <ButtonText
          className="govuk-!-margin-top-2"
          onClick={() => {
            setSearchTerm('');
            toggleHasSubmitted.off();
            toggleError.off();
            onReset?.();
          }}
        >
          Clear search
        </ButtonText>
      )}
    </FormGroup>
  );
};

export default FormSearchBar;
