import styles from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration.module.scss';
import { ChartLegendFormValues } from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration';
import {
  legendInlinePositions,
  colours,
  lineStyles,
  symbols,
} from '@common/modules/charts/util/chartUtils';
import {
  LegendConfiguration,
  LegendPosition,
} from '@common/modules/charts/types/legend';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import RHFFormFieldColourInput from '@common/components/form/rhf/RHFFormFieldColourInput';
import Effect from '@common/components/Effect';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import FormFieldset from '@common/components/form/FormFieldset';
import RHFFormFieldSelect from '@common/components/form/rhf/RHFFormFieldSelect';
import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import { ChartCapabilities } from '@common/modules/charts/types/chart';
import { Dictionary } from '@common/types';
import upperFirst from 'lodash/upperFirst';
import { useFieldArray, useFormContext, useWatch } from 'react-hook-form';
import React from 'react';

const symbolOptions: SelectOption[] = symbols.map(symbol => ({
  label: upperFirst(symbol),
  value: symbol,
}));

const lineStyleOptions: SelectOption[] = lineStyles.map(lineStyle => ({
  label: upperFirst(lineStyle),
  value: lineStyle,
}));

const inlinePositionOptions: SelectOption[] = legendInlinePositions.map(
  position => ({
    label: upperFirst(position),
    value: position,
  }),
);

interface Props {
  capabilities: ChartCapabilities;
  position?: LegendPosition;
  onChange: (legend: LegendConfiguration) => void;
}

export default function ChartLegendItems({
  capabilities,
  position,
  onChange,
}: Props) {
  const { fields } = useFieldArray({
    name: 'items',
  });

  const { formState } = useFormContext<ChartLegendFormValues>();

  const currentItems = useWatch({ name: 'items' });

  const [handleChange] = useDebouncedCallback(() => {
    if (formState.isValid) {
      onChange({ items: currentItems });
    }
  }, 200);

  return (
    <div className="dfe-overflow-x--auto govuk-!-margin-bottom-6">
      {fields.length ? (
        <>
          <Effect
            value={currentItems}
            onChange={handleChange}
            onMount={
              // Update chart builder state with legend items if the
              // chart is using deprecated chart data set configurations
              fields.length > 0 ? handleChange : undefined
            }
          />
          {fields.map((item, index) => {
            const fieldErrors = formState.errors.items?.[index];
            const fieldErrorDetails = fieldErrors
              ? Object.values(fieldErrors as Dictionary<{ message: string }>)
              : [];

            return (
              <div key={item.id} className={styles.item}>
                <FormFieldset
                  id={`items-${item.id}`}
                  legend={`Legend item ${index + 1}`}
                  legendHidden
                  error={
                    fieldErrorDetails[0] ? fieldErrorDetails[0].message : ''
                  }
                >
                  <div className="dfe-flex dfe-justify-content--space-between">
                    <div className={styles.labelInput}>
                      <RHFFormFieldTextInput
                        name={`items.${index}.label`}
                        label="Label"
                        formGroup={false}
                        showError={false}
                      />
                    </div>
                    <div className={styles.colourInput}>
                      <RHFFormFieldColourInput
                        name={`items.${index}.colour`}
                        label="Colour"
                        colours={colours}
                        formGroup={false}
                        showError={false}
                      />
                    </div>

                    {capabilities.hasSymbols && (
                      <div className={styles.configurationInput}>
                        <RHFFormFieldSelect
                          name={`items.${index}.symbol`}
                          label="Symbol"
                          formGroup={false}
                          showError={false}
                          options={symbolOptions}
                        />
                      </div>
                    )}

                    {capabilities.hasLineStyle && (
                      <div className={styles.configurationInput}>
                        <RHFFormFieldSelect
                          name={`items.${index}.lineStyle`}
                          label="Style"
                          order={FormSelect.unordered}
                          formGroup={false}
                          showError={false}
                          options={lineStyleOptions}
                        />
                      </div>
                    )}

                    {position === 'inline' && (
                      <div className={styles.configurationInput}>
                        <RHFFormFieldSelect
                          name={`items.${index}.inlinePosition`}
                          label="Position"
                          order={FormSelect.unordered}
                          formGroup={false}
                          showError={false}
                          options={inlinePositionOptions}
                        />
                      </div>
                    )}
                  </div>
                </FormFieldset>
              </div>
            );
          })}
        </>
      ) : (
        <p>No legend items to customize.</p>
      )}
    </div>
  );
}