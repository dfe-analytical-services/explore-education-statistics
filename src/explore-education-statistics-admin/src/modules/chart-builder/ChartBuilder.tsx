import React from 'react';

import Details from '@common/components/Details';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import { DataBlockResponse } from '@common/services/dataBlockService';
import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import ChartDataSelector, {
  DataUpdatedEvent,
} from '@admin/modules/chart-builder/ChartDataSelector';
import {
  ChartDataSet,
  DataLabelConfigurationItem,
  AxisConfigurationItem,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import styles from './graph-builder.module.scss';
import ConstData from '../../pages/prototypes/PrototypeData';
import ChartTypeSelector from './ChartTypeSelector';
import ChartDataConfiguration from './ChartDataConfiguration';
import ChartAxisConfiguration from './ChartAxisConfiguration';

interface Props {
  data: DataBlockResponse;
}

function getReduceMetaDataForAxis(data: DataBlockResponse) {
  return (
    items: Dictionary<DataLabelConfigurationItem>,
    groupName: string,
  ): Dictionary<DataLabelConfigurationItem> => {
    if (groupName === 'timePeriod') {
      return {
        ...items,
        ...data.result.reduce<Dictionary<DataLabelConfigurationItem>>(
          (moreItems, result) => ({
            ...moreItems,
            [`${result.year}_${result.timeIdentifier}`]: data.metaData
              .timePeriods[`${result.year}_${result.timeIdentifier}`],
          }),
          {},
        ),
      };
    }
    return items;
  };
}

function generateAxesMetaData(
  axes: Dictionary<AxisConfigurationItem>,
  data: DataBlockResponse,
) {
  return Object.values(axes).reduce(
    (allValues, axis) => ({
      ...allValues,
      ...axis.groupBy.reduce(getReduceMetaDataForAxis(data), {}),
    }),
    {},
  );
}

const ChartBuilder = ({ data }: Props) => {
  const [selectedChartType, setSelectedChartType] = React.useState<
    ChartDefinition | undefined
  >();

  const { chartTypes } = ConstData;

  const indicatorIds = Object.keys(data.metaData.indicators);

  const filterIdCombinations: string[][] = Object.values(
    data.result.reduce((filterSet, result) => {
      const filterIds = Array.from(result.filters);

      return {
        ...filterSet,
        [filterIds.join('_')]: filterIds,
      };
    }, {}),
  );

  const [dataSets, setDataSets] = React.useState<ChartDataSet[]>([]);

  const onDataUpdated = (addedData: DataUpdatedEvent[]) => {
    setDataSets(addedData);
  };

  const [labels, setLabels] = React.useState<
    Dictionary<DataLabelConfigurationItem>
  >({});

  const [fieldLabels, setFieldLabels] = React.useState<
    Dictionary<DataLabelConfigurationItem>
  >({});

  const [axes, setAxes] = React.useState<Dictionary<AxisConfigurationItem>>({});

  React.useEffect(() => {
    setLabels({
      ...fieldLabels,

      ...generateAxesMetaData(axes, data),
    });
  }, [axes, fieldLabels, data]);

  React.useEffect(() => {
    if (selectedChartType) {
      const axiConfiguration = selectedChartType.axes.reduce<
        Dictionary<AxisConfigurationItem>
      >(
        (axesConfigurationDictionary, axisDefinition) => ({
          ...axesConfigurationDictionary,

          [axisDefinition.type]: {
            name: axisDefinition.title,
            groupBy: axisDefinition.defaultDataType
              ? [axisDefinition.defaultDataType]
              : [],
            dataSets: axisDefinition.type === 'major' ? dataSets : [],
          },
        }),
        {},
      );

      setAxes(axiConfiguration);
    }
  }, [dataSets, selectedChartType]);

  return (
    <div className={styles.editor}>
      <Details summary="Select chart type" open>
        <ChartTypeSelector
          chartTypes={chartTypes}
          onSelectChart={setSelectedChartType}
          selectedChartType={selectedChartType}
        />
      </Details>

      {selectedChartType && (
        <Details summary="Add data to chart" open>
          <ChartDataSelector
            onDataUpdated={onDataUpdated}
            metaData={data.metaData}
            indicatorIds={indicatorIds}
            filterIds={filterIdCombinations}
            chartType={selectedChartType}
          />
        </Details>
      )}

      {selectedChartType && dataSets.length > 0 && (
        <React.Fragment>
          <Details summary="Chart preview" open>
            <ChartRenderer
              type={selectedChartType.type}
              axes={axes}
              data={data}
              meta={data.metaData}
              labels={labels}
            />
          </Details>

          <Details summary="Data label options">
            <ChartDataConfiguration
              dataSets={dataSets}
              data={data}
              meta={data.metaData}
              onDataLabelsChange={setFieldLabels}
            />
          </Details>

          <Details summary="Axes options" open>
            <p>
              Add / Remove and update the axes and how they display data ranges
            </p>
            {Object.values(axes).map(axis => (
              <ChartAxisConfiguration
                id={axis.name}
                axisConfiguration={axis}
                key={axis.name}
                meta={data.metaData}
              />
            ))}
          </Details>
        </React.Fragment>
      )}
    </div>
  );
};

export default ChartBuilder;
