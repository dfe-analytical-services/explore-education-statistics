import { FormBaseInputProps } from '@common/components/form/FormBaseInput';
import styles from '@common/components/form/FormColourInput.module.scss';
import ModalConfirm from '@common/components/ModalConfirm';
import FormRadioGroup, {
  RadioOption,
} from '@common/components/form/FormRadioGroup';
import Button from '@common/components/Button';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import { Colour } from '@common/modules/charts/util/chartUtils';
import React, { useEffect, useState } from 'react';
import { SketchPicker } from '@hello-pangea/color-picker';

export interface FormColourInputProps extends FormBaseInputProps {
  colours: Colour[];
  itemLabel?: string;
  initialValue?: string;
  onConfirm?: (updatedValue: string) => void;
}

export default function FormColourInput({
  colours = [],
  itemLabel,
  initialValue,
  label,
  name,
  onConfirm,
}: FormColourInputProps) {
  // Select first colour from palette if no initial value.
  const initialColour = initialValue ?? colours[0].value;
  const initialColourFromPalette = colours.find(
    colour => colour.value === initialColour,
  );
  const isCustomColour = !initialColourFromPalette;

  const [showPicker, togglePicker] = useToggle(false);
  const [selectedColour, setSelectedColour] = useState<string>(
    initialColourFromPalette ? initialColourFromPalette.value : 'custom',
  );
  const [selectedCustomColour, setSelectedCustomColour] = useState<
    string | undefined
  >(isCustomColour ? initialColour : undefined);

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

  const options: RadioOption[] = colours.map(colour => {
    return { label: colour.label, value: colour.value, swatch: colour.value };
  });

  return (
    <ModalConfirm
      title="Legend colour"
      triggerButton={
        <button type="button" className={styles.button}>
          <span>{label}</span>
          <VisuallyHidden>
            {` ${
              initialColourFromPalette
                ? initialColourFromPalette.label
                : selectedCustomColour
            }, click to change colour ${itemLabel ? `for ${itemLabel}` : ''}`}
          </VisuallyHidden>
          <div className={styles.inner}>
            <div
              aria-hidden
              className={styles.swatch}
              style={{ backgroundColor: initialColour }}
            />
          </div>
        </button>
      }
      onConfirm={() => onConfirm?.(selectedCustomColour ?? selectedColour)}
    >
      {showPicker && (
        <div className={styles.popover}>
          {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events, jsx-a11y/no-static-element-interactions */}
          <div
            className={styles.cover}
            data-testid="picker-overlay"
            onClick={togglePicker.off}
          />
          <SketchPicker
            color={selectedCustomColour}
            disableAlpha
            presetColors={[]}
            onChange={result => setSelectedCustomColour(result.hex)}
          />
        </div>
      )}
      <FormRadioGroup
        className={styles.coloursList}
        name={`${name}-colour-picker`}
        id={`${name}-colour-picker`}
        legend="Select a colour"
        legendHidden
        value={selectedColour}
        options={[
          ...options,
          {
            label: 'Custom',
            value: 'custom',
            swatch: selectedCustomColour,
          },
        ]}
        order={[]}
        onChange={event => {
          setSelectedColour(event?.target.value);
          if (event?.target.value !== 'custom') {
            setSelectedCustomColour(undefined);
          }
        }}
      />
      {selectedColour === 'custom' && (
        <Button
          className="govuk-!-margin-0"
          variant="secondary"
          onClick={togglePicker.on}
        >
          Select custom colour
        </Button>
      )}
    </ModalConfirm>
  );
}
