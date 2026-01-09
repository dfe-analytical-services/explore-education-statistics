import { FormBaseInputProps } from '@common/components/form/FormBaseInput';
import styles from '@common/components/form/FormColourInput.module.scss';
import ModalConfirm from '@common/components/ModalConfirm';
import FormRadioGroup, {
  RadioOption,
} from '@common/components/form/FormRadioGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { Colour } from '@common/modules/charts/util/chartUtils';
import React, { useState } from 'react';
import { ColorResult, SketchPicker } from '@hello-pangea/color-picker';

export interface FormColourInputProps extends FormBaseInputProps {
  colours: Colour[];
  itemLabel?: string;
  initialValue?: string;
  onConfirm?: (updatedValue: string) => void;
}

export default function FormColourInput({
  colours,
  itemLabel,
  initialValue,
  label,
  name,
  onConfirm,
}: FormColourInputProps) {
  // Select first colour from palette if no initial value.
  const initialColourValue = initialValue ?? colours[0].value;
  const initialColourFromPalette = colours.find(
    colour => colour.value === initialColourValue,
  );
  const initialSelectedValue = initialColourFromPalette
    ? initialColourFromPalette.value
    : 'custom';

  const [selectedColour, setSelectedColour] =
    useState<string>(initialSelectedValue);

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
                : initialColourValue
            }, click to change colour ${itemLabel ? `for ${itemLabel}` : ''}`}
          </VisuallyHidden>
          <div className={styles.inner}>
            <div
              aria-hidden
              className={styles.swatch}
              style={{ backgroundColor: initialColourValue }}
            />
          </div>
        </button>
      }
      onConfirm={() => {
        onConfirm?.(selectedColour);
      }}
    >
      <FormColourInputInner
        colours={colours}
        initialColourValue={initialColourValue}
        initialColourIsCustom={!initialColourFromPalette}
        name={name}
        onChange={value => {
          setSelectedColour(value);
        }}
      />
    </ModalConfirm>
  );
}

interface FormColourInputInnerProps {
  colours: Colour[];
  initialColourValue: string;
  initialColourIsCustom: boolean;
  name: string;
  onChange: (value: string) => void;
}

function FormColourInputInner({
  colours,
  initialColourValue,
  initialColourIsCustom,
  name,
  onChange,
}: FormColourInputInnerProps) {
  const [selectedColourOption, setSelectedColourOption] = useState<string>(
    initialColourIsCustom ? 'custom' : initialColourValue,
  );
  const [selectedCustomColour, setSelectedCustomColour] = useState<
    string | undefined
  >(initialColourIsCustom ? initialColourValue : undefined);
  const options: RadioOption[] = colours.map(colour => {
    return { label: colour.label, value: colour.value, swatch: colour.value };
  });
  const getCustomColourValue = (selectedValue: string) => {
    if (selectedValue !== 'custom') {
      return undefined;
    }
    return !selectedCustomColour ? colours[0].value : selectedValue;
  };
  const handleCustomColourChange = (result: ColorResult) => {
    setSelectedCustomColour(result.hex);
    onChange(result.hex);
  };

  return (
    <>
      <FormRadioGroup
        className={styles.coloursList}
        name={`${name}-colour-picker`}
        id={`${name}-colour-picker`}
        legend="Select a colour"
        legendHidden
        value={selectedColourOption}
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
          const value = event?.target.value;
          const customColour = getCustomColourValue(value);
          setSelectedColourOption(value);
          setSelectedCustomColour(customColour);
          onChange(customColour ?? value);
        }}
      />

      {selectedColourOption === 'custom' && (
        <>
          <h3>Choose a custom colour:</h3>
          {/* Disable the colour picker for tests as it causes errors */}
          {process.env.NODE_ENV !== 'test' && (
            <SketchPicker
              color={selectedCustomColour}
              disableAlpha
              presetColors={[]}
              onChange={handleCustomColourChange}
            />
          )}
        </>
      )}
    </>
  );
}
