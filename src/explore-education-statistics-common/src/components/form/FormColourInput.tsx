import BaseFormInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import styles from '@common/components/form/FormColourInput.module.scss';
import useToggle from '@common/hooks/useToggle';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { useEffect } from 'react';
import { SketchPicker, ColorResult } from '@hello-pangea/color-picker';
import { useFormContext } from 'react-hook-form';

export interface FormColourInputProps extends FormBaseInputProps {
  colours?: string[];
  value?: string;
}

export default function FormColourInput({
  colours = [],
  name,
  ...props
}: FormColourInputProps) {
  const { getValues, setValue } = useFormContext();
  const value = getValues(name);
  const [showPicker, togglePicker] = useToggle(false);

  useEffect(() => {
    const handleWindowKeyDown = (event: KeyboardEvent) => {
      if (showPicker && event.key === 'Escape') {
        togglePicker.off();
      }
    };
    window.addEventListener('keydown', handleWindowKeyDown);

    return () => {
      window.removeEventListener('keydown', handleWindowKeyDown);
    };
  }, [showPicker, togglePicker]);

  const handleChange = (colour: ColorResult) => {
    setValue(name, colour.hex);
  };

  return (
    <>
      <BaseFormInput {...props} name={name} value={value} />
      <button type="button" className={styles.button} onClick={togglePicker}>
        <div className={styles.swatch} style={{ backgroundColor: value }} />
        <VisuallyHidden>Select colour</VisuallyHidden>
      </button>
      {showPicker && (
        <div className={styles.popover}>
          {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
          <div className={styles.cover} onClick={togglePicker.off} />
          <SketchPicker
            color={value}
            presetColors={colours}
            onChange={handleChange}
          />
        </div>
      )}
    </>
  );
}
