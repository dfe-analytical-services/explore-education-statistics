import styles from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration.module.scss';
import { ChartLegendFormValues } from '@admin/pages/release/datablocks/components/chart/ChartLegendConfiguration';
import ChartReorderCategories from '@admin/pages/release/datablocks/components/chart/ChartReorderCategories';
import {
  legendInlinePositions,
  colours,
  lineStyles,
  symbols,
  legendLabelColours,
} from '@common/modules/charts/util/chartUtils';
import {
  LegendConfiguration,
  LegendPosition,
} from '@common/modules/charts/types/legend';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormFieldColourInput from '@common/components/form/FormFieldColourInput';
import Effect from '@common/components/Effect';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldset from '@common/components/form/FormFieldset';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import { FormFieldCheckbox } from '@common/components/form';
import { DataSet } from '@common/modules/charts/types/dataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import {
  ChartCapabilities,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
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

const legendLabelColourOptions: SelectOption[] = legendLabelColours.map(
  colour => ({
    label: upperFirst(colour),
    value: colour,
  }),
);

const inlinePositionOptions: SelectOption[] = legendInlinePositions.map(
  position => ({
    label: upperFirst(position),
    value: position,
  }),
);

interface Props {
  capabilities: ChartCapabilities;
  mapDataSetConfigs?: MapDataSetConfig[];
  position?: LegendPosition;
  onChange: (legend: LegendConfiguration) => void;
  onReorderCategories: (mapDataSetConfig: MapDataSetConfig) => void;
}

export default function ChartLegendItems({
  capabilities,
  mapDataSetConfigs,
  position,
  onChange,
  onReorderCategories,
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

  const getMapDataSetConfig = (dataSet?: DataSet) => {
    if (!dataSet) {
      return undefined;
    }
    const key = generateDataSetKey(dataSet);
    return mapDataSetConfigs?.find(config => config.dataSetKey === key);
  };

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
            const mapDataSetConfig = getMapDataSetConfig(
              currentItems[index]?.dataSet,
            );
            const allowColourSelection =
              !mapDataSetConfig?.categoricalDataConfig?.length ||
              currentItems[index].sequentialCategoryColours;
            return (
              <div
                key={item.id}
                className={styles.item}
                data-testid={`legend-item-${index}`}
              >
                <FormFieldset
                  id={`items-${item.id}`}
                  legend={`Legend item ${index + 1}`}
                  legendHidden
                  error={
                    fieldErrorDetails[0] ? fieldErrorDetails[0].message : ''
                  }
                >
                  <div className="dfe-flex">
                    <div className={styles.labelInput}>
                      <FormFieldTextInput
                        name={`items.${index}.label`}
                        label="Label"
                        formGroup={false}
                        showError={false}
                      />
                    </div>
                    {mapDataSetConfig?.categoricalDataConfig && (
                      <div className={styles.labelInput}>
                        <FormFieldCheckbox
                          label="Sequential colours"
                          name={`items.${index}.sequentialCategoryColours`}
                          formGroup={false}
                        />
                      </div>
                    )}
                    {allowColourSelection && (
                      <div className={styles.colourInput}>
                        <FormFieldColourInput
                          name={`items.${index}.colour`}
                          label="Colour"
                          itemLabel={currentItems[index].label}
                          colours={colours}
                          formGroup={false}
                          showError={false}
                        />
                      </div>
                    )}

                    {capabilities.hasLineStyle && position === 'inline' && (
                      <div className={styles.configurationInput}>
                        <FormFieldSelect
                          name={`items.${index}.labelColour`}
                          label="Label Colour"
                          formGroup={false}
                          showError={false}
                          options={legendLabelColourOptions}
                        />
                      </div>
                    )}

                    {capabilities.hasSymbols && (
                      <div className={styles.configurationInput}>
                        <FormFieldSelect
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
                        <FormFieldSelect
                          name={`items.${index}.lineStyle`}
                          label="Style"
                          order={FormSelect.unordered}
                          formGroup={false}
                          showError={false}
                          options={lineStyleOptions}
                        />
                      </div>
                    )}

                    {capabilities.hasLineStyle && position === 'inline' && (
                      <>
                        <div className={styles.configurationInput}>
                          <FormFieldSelect
                            name={`items.${index}.inlinePosition`}
                            label="Position"
                            order={FormSelect.unordered}
                            formGroup={false}
                            showError={false}
                            options={inlinePositionOptions}
                          />
                        </div>
                        <div className={styles.numberInput}>
                          <FormFieldNumberInput
                            name={`items.${index}.inlinePositionOffset`}
                            label="Y Offset"
                            formGroup={false}
                            showError={false}
                            min={-100}
                            max={100}
                          />
                        </div>
                      </>
                    )}
                    {capabilities.canReorderDataCategories &&
                      mapDataSetConfig &&
                      mapDataSetConfig.categoricalDataConfig && (
                        <ChartReorderCategories
                          label={currentItems[index].label}
                          mapDataSetConfig={mapDataSetConfig}
                          onReorder={updatedConfig =>
                            onReorderCategories(updatedConfig)
                          }
                        />
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
