import { ChartDataSetAndConfiguration } from '@admin/modules/chart-builder/ChartDataSelector';
import {
  FormCheckbox,
  FormFieldset,
  FormGroup,
  FormRadioGroup,
  FormTextInput,
} from '@common/components/form';
import FormComboBox from '@common/components/form/FormComboBox';

import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import { ChartCapabilities } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import {
  AxisConfiguration,
  AxisGroupBy,
  ReferenceLine,
} from '@common/services/publicationService';
import * as React from 'react';
import styles from './graph-builder.module.scss';

interface Props {
  id: string;
  defaultDataType?: AxisGroupBy;
  configuration: AxisConfiguration;
  meta: DataBlockMetadata;
  capabilities: ChartCapabilities;
  chartDataConfiguration: ChartDataSetAndConfiguration[];
  onConfigurationChange: (configuration: AxisConfiguration) => void;
}

const getSelectableUnits = (
  configuration: AxisConfiguration,
  meta: DataBlockMetadata,
) => {
  return configuration.dataSets
    .map(dataSet => meta.indicators[dataSet.indicator])
    .filter(indicator => indicator !== null)
    .map(indicator => indicator.unit);
};

const getSortOptions = (
  chartDataConfiguration: ChartDataSetAndConfiguration[],
): SelectOption[] => {
  return [
    {
      label: 'default',
      value: 'default',
    },
    ...chartDataConfiguration.map<SelectOption>(config => ({
      label: config.configuration.label,
      value: config.configuration.value,
    })),
  ];
};

const ChartAxisConfiguration = ({
  id,
  configuration,
  meta,
  capabilities,
  onConfigurationChange,
  chartDataConfiguration,
}: Props) => {
  const [axisConfiguration, setAxisConfiguration] = React.useState<
    AxisConfiguration
  >(configuration);

  const [selectableUnits, setSelectableUnits] = React.useState<string[]>(() => {
    return getSelectableUnits(configuration, meta);
  });

  const [sortOptions, setSortOptions] = React.useState<SelectOption[]>(() =>
    getSortOptions(chartDataConfiguration),
  );

  React.useEffect(() => {
    setAxisConfiguration(configuration);
    setSelectableUnits(getSelectableUnits(configuration, meta));
    setSortOptions(getSortOptions(chartDataConfiguration));
  }, [configuration, meta, chartDataConfiguration]);

  const [selectedUnit] = React.useState<number>(0);

  const [selectedValue, setSelectedValue] = React.useState<string>();

  const updateAxisConfiguration = (newValues: object) => {
    const newConfiguration = { ...axisConfiguration, ...newValues };
    setAxisConfiguration(newConfiguration);
    if (onConfigurationChange) onConfigurationChange(newConfiguration);
  };

  const [referenceLine, setReferenceLine] = React.useState<ReferenceLine>({
    position: '',
    label: '',
  });
  const [referenceOptions] = React.useState<SelectOption[]>(() => {
    if (axisConfiguration.groupBy) {
      return [
        { label: 'Select', value: '' },
        ...Object.values(meta[axisConfiguration.groupBy]).map(({ label }) => ({
          label,
          value: label,
        })),
      ];
    }
    return [];
  });

  return (
    <div className={styles.chartAxesConfiguration}>
      <form>
        <FormFieldset id={id} legend={axisConfiguration.title}>
          <h2 className="govuk-heading-s">
            {axisConfiguration.name} configuration
          </h2>
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
                      inputLabel="Display Unit"
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

            <hr />

            {capabilities.gridLines && (
              <React.Fragment>
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
                <hr />
              </React.Fragment>
            )}

            <FormTextInput
              id={`${id}_size`}
              name={`${id}_size`}
              type="number"
              min="0"
              max="100"
              label="Size of axis"
              value={axisConfiguration.size}
              onChange={e => updateAxisConfiguration({ size: e.target.value })}
            />
            <hr />

            {axisConfiguration.type === 'minor' && (
              <>
                <FormFieldset
                  id="axis_range"
                  legend="Axis range"
                  legendSize="s"
                >
                  <p>Leaving these blank will set it to 'auto'</p>
                  <div className={styles.axisRange}>
                    <FormGroup className={styles.formGroup}>
                      <FormTextInput
                        id={`${id}_min`}
                        name={`${id}_min`}
                        type="number"
                        width={10}
                        label="Minimum value"
                        value={axisConfiguration.min}
                        onChange={e =>
                          updateAxisConfiguration({ min: e.target.value })
                        }
                      />
                    </FormGroup>
                    <FormGroup className={styles.formGroup}>
                      <FormTextInput
                        id={`${id}_max`}
                        name={`${id}_max`}
                        type="number"
                        width={10}
                        label="Maximum Value"
                        value={axisConfiguration.max}
                        onChange={e =>
                          updateAxisConfiguration({ max: e.target.value })
                        }
                      />
                    </FormGroup>
                  </div>
                </FormFieldset>
                <hr />
              </>
            )}

            <FormRadioGroup
              id={`${id}_tick_type`}
              name="tick_Type"
              legend="Tick display type"
              legendSize="s"
              value={axisConfiguration.tickConfig}
              onChange={e => {
                updateAxisConfiguration({ tickConfig: e.target.value });
              }}
              order={[]}
              options={[
                {
                  value: 'default',
                  label: 'Automatic',
                },
                {
                  label: 'Start and end only',
                  value: 'startEnd',
                },
                {
                  label: 'Custom',
                  value: 'custom',
                  conditional: (
                    <FormTextInput
                      id={`${id}_tick_spacing`}
                      name={`${id}_tick_spacing`}
                      type="number"
                      width={10}
                      label="Every nth value"
                      value={axisConfiguration.tickSpacing}
                      onChange={e =>
                        updateAxisConfiguration({ tickSpacing: e.target.value })
                      }
                    />
                  ),
                },
              ]}
            />
            <hr />

            {axisConfiguration.type === 'major' && (
              <>
                <FormFieldset id={`${id}sort_order_set`} legend="Sort order">
                  <FormGroup>
                    <FormSelect
                      id={`${id}_sort_by`}
                      name="sort_by"
                      label="Sort By"
                      order={[]}
                      value={axisConfiguration.sortBy}
                      onChange={e => {
                        updateAxisConfiguration({ sortBy: e.target.value });
                      }}
                      options={sortOptions}
                    />
                    <FormCheckbox
                      id={`${id}_sort_asc`}
                      name="sort_asc"
                      label="Sort Ascending"
                      value="asc"
                      checked={axisConfiguration.sortAsc}
                      onChange={e => {
                        updateAxisConfiguration({ sortAsc: e.target.checked });
                      }}
                    />
                  </FormGroup>
                </FormFieldset>
                <hr />
              </>
            )}

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

            <table className="govuk-table">
              <caption className="govuk-caption-m">Reference lines</caption>
              <thead>
                <tr>
                  <th>Position</th>
                  <th>Label</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {axisConfiguration.referenceLines &&
                  axisConfiguration.referenceLines.map((rl, idx) => (
                    <tr key={`${rl.label}_${rl.position}`}>
                      <td>{rl.position}</td>
                      <td>{rl.label}</td>
                      <td>
                        <button
                          className="govuk-button govuk-button--secondary govuk-!-margin-0"
                          type="button"
                          onClick={() => {
                            const newReferenceLines = [
                              ...(axisConfiguration.referenceLines || []),
                            ];
                            newReferenceLines.splice(idx, 1);
                            updateAxisConfiguration({
                              referenceLines: newReferenceLines,
                            });
                          }}
                        >
                          Remove
                        </button>
                      </td>
                    </tr>
                  ))}
                <tr>
                  <td>
                    {axisConfiguration.type === 'minor' && (
                      <FormTextInput
                        name=""
                        id=""
                        label=""
                        type="text"
                        value={`${referenceLine.position}`}
                        onChange={e => {
                          setReferenceLine({
                            ...referenceLine,
                            position: e.target.value,
                          });
                        }}
                      />
                    )}
                    {axisConfiguration.type === 'major' && (
                      <FormSelect
                        name=""
                        id=""
                        label=""
                        value={referenceLine.position}
                        order={[]}
                        onChange={e => {
                          setReferenceLine({
                            ...referenceLine,
                            position: e.target.value,
                          });
                        }}
                        options={referenceOptions}
                      />
                    )}
                  </td>
                  <td>
                    <FormTextInput
                      name=""
                      id=""
                      label=""
                      type="text"
                      value={referenceLine.label}
                      onChange={e => {
                        setReferenceLine({
                          ...referenceLine,
                          label: e.target.value,
                        });
                      }}
                    />
                  </td>
                  <td>
                    <button
                      className="govuk-button govuk-!-margin-bottom-0"
                      type="button"
                      onClick={() => {
                        updateAxisConfiguration({
                          referenceLines: [
                            ...(axisConfiguration.referenceLines || []),
                            referenceLine,
                          ],
                        });
                        setReferenceLine({ label: '', position: '' });
                      }}
                    >
                      Add
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </FormGroup>
        </FormFieldset>
      </form>
    </div>
  );
};

export default ChartAxisConfiguration;
