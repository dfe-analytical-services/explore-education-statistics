import * as React from 'react';
import {
  FormCheckbox,
  FormFieldset,
  FormGroup,
  FormTextInput,
} from '@common/components/form';
import FormComboBox from '@common/components/form/FormComboBox';
import {
  AxisConfiguration,
  AxisGroupBy,
} from '@common/services/publicationService';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import { ChartCapabilities } from '@common/modules/find-statistics/components/charts/ChartFunctions';

interface Props {
  id: string;
  defaultDataType?: AxisGroupBy;
  configuration: AxisConfiguration;
  meta: DataBlockMetadata;
  capabilities: ChartCapabilities;
  onConfigurationChange: (configuration: AxisConfiguration) => void;
}

const ChartAxisConfiguration = ({
  id,
  configuration,
  meta,
  capabilities,
  onConfigurationChange,
}: Props) => {
  const [axisConfiguration, setAxisConfiguration] = React.useState<
    AxisConfiguration
  >(configuration);

  const [selectableUnits] = React.useState<string[]>(() => {
    return configuration.dataSets
      .map(dataSet => meta.indicators[dataSet.indicator])
      .filter(indicator => indicator !== null)
      .map(indicator => indicator.unit);
  });

  const [selectedUnit] = React.useState<number>(0);

  const [selectedValue, setSelectedValue] = React.useState<string>();

  const updateAxisConfiguration = (newValues: object) => {
    const newConfiguration = { ...axisConfiguration, ...newValues };
    setAxisConfiguration(newConfiguration);
    if (onConfigurationChange) onConfigurationChange(newConfiguration);
  };

  return (
    <FormFieldset id={id} legend={axisConfiguration.title}>
      <p>{axisConfiguration.name} configuration</p>
      <FormGroup>
        <FormCheckbox
          id={`${id}_show`}
          name={`${id}_show`}
          label="Show axis?"
          checked={axisConfiguration.visible}
          onChange={e => {
            updateAxisConfiguration({ visible: e.target.checked });
          }}
          value="show"
          conditional={
            <React.Fragment>
              {axisConfiguration.type === 'major' && (
                <FormComboBox
                  id={`${id}_unit`}
                  inputLabel="Unit"
                  onInputChange={e => setSelectedValue(e.target.value)}
                  inputValue={selectedValue}
                  onSelect={selected => {
                    setSelectedValue(selectableUnits[selected]);
                  }}
                  options={selectableUnits}
                  initialOption={selectedUnit}
                />
              )}
            </React.Fragment>
          }
        />
        {capabilities.gridLines && (
          <FormCheckbox
            id={`${id}_grid`}
            name={`${id}_grid`}
            label="Show grid lines"
            onChange={e =>
              updateAxisConfiguration({ showGrid: e.target.checked })
            }
            checked={axisConfiguration.showGrid}
            value="grid"
          />
        )}

        <FormTextInput
          id={`${id}_size`}
          name={`${id}_size`}
          type="number"
          min="0"
          max="100"
          label="Size"
          value={axisConfiguration.size}
          onChange={e => updateAxisConfiguration({ size: e.target.value })}
        />

        {/*
        <FormSelect
          id={`${id}_labelPosition`}
          name={`${id}_labelPosition`}
          label="Label position"
          onChange={e =>
            updateAxisConfiguration({ labelPosition: e.target.value })
          }
          options={[
            { label: 'On axis', value: 'axis' },
            { label: 'On graph', value: 'graph' },
          ]}
        />
*/}

        <p>Restrict range of series (years only?) DFE-1009</p>
      </FormGroup>
    </FormFieldset>
  );
};

export default ChartAxisConfiguration;
