import {
  FormCheckbox,
  FormFieldset,
  FormGroup,
  FormRadioGroup,
  FormTextInput,
} from '@common/components/form';

import FormSelect, { SelectOption } from '@common/components/form/FormSelect';
import {
  ChartCapabilities,
  ChartDataB,
  ChartMetaData,
  createSortedAndMappedDataForAxis,
  getNiceMaxValue,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { DataBlockData } from '@common/services/dataBlockService';
import {
  AxisConfiguration,
  AxisGroupBy,
  ChartDataSet,
  DataSetConfiguration,
  ReferenceLine,
} from '@common/services/publicationService';
import * as React from 'react';
import { Dictionary } from '@common/types';
import styles from './graph-builder.module.scss';

interface Props {
  id: string;
  defaultDataType?: AxisGroupBy;
  configuration: AxisConfiguration;
  data: DataBlockData;
  meta: ChartMetaData;
  labels: Dictionary<DataSetConfiguration>;
  capabilities: ChartCapabilities;
  onConfigurationChange: (configuration: AxisConfiguration) => void;
  dataSets: ChartDataSet[];
}

const getSortOptions = (
  labels: Dictionary<DataSetConfiguration>,
): SelectOption[] => {
  return [
    {
      label: 'default',
      value: 'name',
    },
    ...Object.values(labels).map<SelectOption>(config => ({
      label: config.label,
      value: config.value,
    })),
  ];
};

const getAxisLabels = (
  configuration: AxisConfiguration,
  data: DataBlockData,
  meta: ChartMetaData,
  labels: Dictionary<DataSetConfiguration>,
  dataSets: ChartDataSet[],
): SelectOption[] => {
  const configurationWithDataSet: AxisConfiguration = {
    ...configuration,
    dataRange: [undefined, undefined],
    dataSets,
  };

  const chartData: ChartDataB[] = createSortedAndMappedDataForAxis(
    configurationWithDataSet,
    data.result,
    meta,
    labels,
  );

  return [
    {
      label: 'default',
      value: '',
    },
    ...chartData.map(({ name }, index) => ({
      label: name,
      value: `${index}`,
    })),
  ];
};

const ChartAxisConfiguration = ({
  id,
  configuration,
  data,
  meta,
  labels,
  capabilities,
  onConfigurationChange,
  dataSets = [],
}: Props) => {
  const [axisConfiguration, setAxisConfiguration] = React.useState<
    AxisConfiguration
  >(configuration);

  const [sortOptions, setSortOptions] = React.useState<SelectOption[]>(() =>
    getSortOptions(labels),
  );

  React.useEffect(() => {
    setAxisConfiguration(configuration);
  }, [configuration]);

  const [limitOptions, setLimitOptions] = React.useState<SelectOption[]>(() =>
    getAxisLabels(configuration, data, meta, labels, dataSets),
  );

  const [dataRangeMin, setDataRangeMin] = React.useState<string>(
    `${configuration.dataRange || [''][0]}`,
  );
  const [dataRangeMax, setDataRangeMax] = React.useState<string>(
    `${configuration.dataRange || ['', ''][1]}`,
  );

  const updateAxisConfiguration = (newValues: object) => {
    const newConfiguration = { ...axisConfiguration, ...newValues };
    setAxisConfiguration(newConfiguration);
    if (onConfigurationChange) onConfigurationChange(newConfiguration);
  };

  const updateDataRangeMin = (value: string) => {
    setDataRangeMin(value);
    const valueOrUndef = value === '' ? undefined : value;
    updateAxisConfiguration({
      dataRange: [
        valueOrUndef,
        configuration.dataRange && configuration.dataRange[1],
      ],
    });
  };

  const updateDataRangeMax = (value: string) => {
    setDataRangeMax(value);
    const valueOrUndef =
      value === '' ? undefined : Number.parseInt(value, 10) + 1;
    updateAxisConfiguration({
      dataRange: [
        configuration.dataRange && configuration.dataRange[0],
        valueOrUndef,
      ],
    });
  };

  const getReasonableMinTickSpacing = (): number => {
    if (axisConfiguration) {
      const { min, max } = axisConfiguration;
      return getNiceMaxValue(Math.floor((Number(max) - Number(min)) / 100));
    }
    return 1;
  };

  const getTickSpacing = (): number => {
    // to avoid rendering a ridiculous amount of ticks
    // we conditionally return a reasonableTickSpacing
    if (axisConfiguration) {
      const { tickSpacing } = axisConfiguration;
      const reasonableTickSpacing = getReasonableMinTickSpacing();
      if (tickSpacing) {
        return Number(tickSpacing) > reasonableTickSpacing
          ? Number(tickSpacing)
          : reasonableTickSpacing;
      }
    }
    return 1;
  };

  React.useEffect(() => {
    setAxisConfiguration(configuration);
    setSortOptions(getSortOptions(labels));

    setLimitOptions(getAxisLabels(configuration, data, meta, labels, dataSets));

    // updateAxisConfiguration({dataRange: configuration.dataRange});
  }, [configuration, data, meta, labels, dataSets]);

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
          <FormGroup>
            {axisConfiguration.type === 'major' &&
              !capabilities.fixedAxisGroupBy && (
                <>
                  <FormSelect
                    id={`${id}_groupBy`}
                    label="Group data by"
                    name={`${id}_groupBy`}
                    value={axisConfiguration.groupBy}
                    onChange={e => {
                      updateAxisConfiguration({ groupBy: e.target.value });
                    }}
                    options={[
                      {
                        label: 'Time periods',
                        value: 'timePeriod' as AxisGroupBy,
                      },
                      {
                        label: 'Locations',
                        value: 'locations' as AxisGroupBy,
                      },
                      {
                        label: 'Indicators',
                        value: 'indicators' as AxisGroupBy,
                      },
                      {
                        label: 'Filters',
                        value: 'filters' as AxisGroupBy,
                      },
                    ]}
                  />

                  <hr />
                </>
              )}

            {capabilities.hasAxes && (
              <>
                <FormCheckbox
                  id={`${id}_show`}
                  name={`${id}_show`}
                  label="Show axis labels?"
                  checked={axisConfiguration.visible}
                  onChange={e => {
                    updateAxisConfiguration({ visible: e.target.checked });
                  }}
                  value="show"
                  conditional={
                    <React.Fragment>
                      <FormTextInput
                        id={`${id}_unit`}
                        label="Override displayed unit"
                        name="unit"
                        hint="Leave blank to set default from metadata"
                        width={10}
                        onChange={e =>
                          updateAxisConfiguration({ unit: e.target.value })
                        }
                        defaultValue={axisConfiguration.unit}
                      />
                    </React.Fragment>
                  }
                />
                <hr />

                <FormTextInput
                  id={`${id}_size`}
                  name={`${id}_size`}
                  type="number"
                  min="0"
                  max="100"
                  label="Size of axis (px)"
                  width={3}
                  defaultValue={axisConfiguration.size}
                  onChange={e =>
                    updateAxisConfiguration({ size: e.target.value })
                  }
                />
                <hr />
              </>
            )}

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

            {axisConfiguration.type === 'minor' && (
              <React.Fragment>
                <FormFieldset
                  id="axis_range"
                  legend="Axis range"
                  legendSize="m"
                >
                  <p className="govuk-hint">
                    Leaving these values blank will set them to 'auto'
                  </p>
                  <div className={styles.axisRange}>
                    <FormGroup className={styles.formGroup}>
                      <FormTextInput
                        id={`${id}_min`}
                        name={`${id}_min`}
                        type="number"
                        width={10}
                        label="Minimum value"
                        defaultValue={axisConfiguration.min}
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
                        defaultValue={axisConfiguration.max}
                        onChange={e =>
                          updateAxisConfiguration({ max: e.target.value })
                        }
                      />
                    </FormGroup>
                  </div>
                </FormFieldset>
                <hr />
              </React.Fragment>
            )}

            {capabilities.hasAxes && (
              <>
                <FormRadioGroup
                  id={`${id}_tick_type`}
                  name="tick_Type"
                  legend="Tick display type"
                  legendSize="m"
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
                          defaultValue={`${getTickSpacing()}`}
                          onChange={e => {
                            const theValue = parseInt(e.target.value, 10);
                            if (theValue > 0 && e.target.value !== '') {
                              const reasonableMinTickSpacing = getReasonableMinTickSpacing();
                              updateAxisConfiguration({
                                tickSpacing: `${
                                  theValue > reasonableMinTickSpacing
                                    ? theValue
                                    : reasonableMinTickSpacing
                                }`,
                              });
                            } else {
                              updateAxisConfiguration({
                                tickSpacing: `${getTickSpacing()}`,
                              });
                            }
                          }}
                        />
                      ),
                    },
                  ]}
                />
                <hr />
              </>
            )}

            {axisConfiguration.type === 'major' && (
              <React.Fragment>
                <FormFieldset
                  id={`${id}sort_order_set`}
                  legend="Sorting"
                  legendSize="m"
                >
                  <FormGroup>
                    <FormSelect
                      id={`${id}_sort_by`}
                      name="sort_by"
                      label="Sort data by"
                      order={[]}
                      value={axisConfiguration.sortBy}
                      onChange={e => {
                        updateAxisConfiguration({ sortBy: e.target.value });
                      }}
                      options={sortOptions}
                    />
                  </FormGroup>
                  <FormGroup>
                    <FormCheckbox
                      id={`${id}_sort_asc`}
                      name="sort_asc"
                      label="Sort Ascending"
                      value="asc"
                      defaultChecked={axisConfiguration.sortAsc}
                      onChange={e => {
                        updateAxisConfiguration({ sortAsc: e.target.checked });
                      }}
                    />
                  </FormGroup>
                </FormFieldset>

                <hr />

                <FormFieldset
                  id={`${id}sort_order_set`}
                  legend="Limiting data"
                  legendSize="m"
                >
                  <FormGroup>
                    <FormSelect
                      id={`${id}dataRangeMin`}
                      label="Minimum"
                      name="minimum"
                      value={dataRangeMin}
                      options={limitOptions}
                      order={[]}
                      onChange={e => updateDataRangeMin(e.target.value)}
                    />
                  </FormGroup>
                  <FormGroup>
                    <FormSelect
                      id={`${id}dataRangeMin`}
                      label="Maximum"
                      name="maximum"
                      value={dataRangeMax}
                      options={limitOptions}
                      order={[]}
                      onChange={e => updateDataRangeMax(e.target.value)}
                    />
                  </FormGroup>
                </FormFieldset>

                <hr />
              </React.Fragment>
            )}

            {capabilities.hasReferenceLines && (
              <table className="govuk-table">
                <caption className="govuk-heading-m">Reference lines</caption>
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
                          defaultValue={`${referenceLine.position}`}
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
                        defaultValue={referenceLine.label}
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
                        disabled={
                          referenceLine.position === '' ||
                          referenceLine.label === ''
                        }
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
            )}
          </FormGroup>
        </FormFieldset>
      </form>
    </div>
  );
};

export default ChartAxisConfiguration;
