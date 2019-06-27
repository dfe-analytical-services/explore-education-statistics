import React from 'react';

import Details from '@common/components/Details';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import DataBlockService, {
  GeographicLevel,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import { ChartDefinition } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import ChartDataSelector, {
  DataAddedEvent,
} from '@admin/modules/chart-builder/ChartDataSelector';
import { ChartDataSet } from '@common/services/publicationService';
import styles from './graph-builder.module.scss';
import ConstData from '../../pages/prototypes/PrototypeData';
import ChartTypeSelector from './ChartTypeSelector';

const ChartBuilder = () => {
  const [Data, updateData] = React.useState<DataBlockResponse | undefined>(
    undefined,
  );

  const { chartTypes } = ConstData;
  const [selectedChartType, selectChartType] = React.useState<
    ChartDefinition | undefined
  >();

  const [indicatorIds, setIndicatorIds] = React.useState<string[]>([]);

  const [filterIdCombinations, setFilterIdCombinations] = React.useState<
    string[][]
  >([]);

  React.useEffect(() => {
    const fetchData = async () => {
      const newData = await DataBlockService.getDataBlockForSubject({
        subjectId: 1,
        startYear: '2012',
        endYear: '2016',
        filters: ['1', '71', '72', '73'],
        geographicLevel: GeographicLevel.National,
        indicators: ['23', '26', '28'],
      });
      updateData(newData);

      setIndicatorIds(Object.keys(newData.metaData.indicators));

      const uniqueFilterIds: string[][] = Object.values(
        newData.result.reduce((filterSet, result) => {
          const filterIds = Array.from(result.filters);

          return {
            ...filterSet,
            [filterIds.join('_')]: filterIds,
          };
        }, {}),
      );

      setFilterIdCombinations(uniqueFilterIds);
    };

    fetchData();
  }, []);

  React.useEffect(() => {
    if (selectedChartType) {
      //
    }
  }, [selectedChartType]);

  const [dataSets, setDataSets] = React.useState<ChartDataSet[]>([]);

  const onDataAddedToChart = (data: DataAddedEvent[]) => {
    setDataSets(data);
  };

  if (Data === undefined) return <div />;

  return (
    <div className={styles.editor}>
      <Details summary="Select chart type" open>
        <ChartTypeSelector
          chartTypes={chartTypes}
          onSelectChart={selectChartType}
          selectedChartType={selectedChartType}
        />
      </Details>

      {selectedChartType && (
        <Details summary="Add data to chart" open>
          <ChartDataSelector
            onDataUpdated={data => onDataAddedToChart(data)}
            metaData={Data.metaData}
            indicatorIds={indicatorIds}
            filterIds={filterIdCombinations}
            chartType={selectedChartType}
          />
        </Details>
      )}

      {selectedChartType && dataSets.length > 0 && (
        <Details summary="Chart preview" open>
          <ChartRenderer
            type={selectedChartType.type}
            dataSets={dataSets}
            data={Data}
            meta={Data.metaData}
            xAxis={{ title: '' }}
            yAxis={{ title: '' }}
          />
        </Details>
      )}
    </div>
  );
};

export default ChartBuilder;
