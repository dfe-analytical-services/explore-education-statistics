/* eslint-disable @typescript-eslint/no-unused-vars */
import React, {Fragment} from 'react';

import classnames from 'classnames';

import Details from '@common/components/Details';
import useToggle from '@common/hooks/useToggle';
import ChartRenderer from '@common/modules/find-statistics/components/ChartRenderer';
import DataBlockService, {
  GeographicLevel,
  DataBlockResponse,
} from '@common/services/dataBlockService';
import {ChartDefinition} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import ChartDataSelector from "@admin/pages/prototypes/components/graph-builder/ChartDataSelector";
import styles from './graph-builder/graph-builder.module.scss';
import ConstData from '../PrototypeData';
import ChartTypeSelector from './graph-builder/ChartTypeSelector';
import {ChartDataSet} from '@common/services/publicationService';

/**
 * TODO:
 *
 * Change the charts to use a "Data source" instead of explicitly defining the x/y axis
 * so that they can pull data from indicators / filters instead of specifying x/y
 *
 */
const PrototypeChartEditor = (props: {}) => {
  const [Data, updateData] = React.useState<DataBlockResponse | undefined>(
    undefined,
  );

  const {chartTypes} = ConstData;
  const [selectedChartType, selectChartType] = React.useState<ChartDefinition | undefined>();


  const [selectedIndicators, selectIndicators] = React.useState(
    new Array<string>(),
  );

  const [indicatorIds, setIndicatorIds] = React.useState<string[]>([]);

  const [selectedAxes, updateSelectedAxes] = React.useState<string[][]>([]);

  const [useLegend, setUseLegend] = useToggle(false);

  const showMixedUnits = (): boolean => {
    if (Data && selectedIndicators) {
      return (
        new Set(
          selectedIndicators.map(
            selected => Data.metaData.indicators[selected].unit,
          ),
        ).size > 1
      );
    }
    return false;
  };

  const [filterIdCombinations, setFilterIdCombinations] = React.useState<string[][]>([]);

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
            [filterIds.join('_')]: filterIds
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
    ;

  }, [selectedChartType])


  const isValidChartOptions = () => {
    if (selectedChartType) {
      if (selectedIndicators.length === 0) {
        return false;
      }

      if (selectedChartType.data.length > 0) {
        return (
          selectedAxes &&
          selectedAxes.length > 0 &&
          selectedAxes[0].length !== 0
        );
      }

      return true;
    }

    return false;
  };

  const [dataSets, setDataSets] = React.useState<ChartDataSet[]>([]);

  const [indicators, setIndicators] = React.useState<string[]>([]);

  const onDataAddedToChart = ({filters, indicator}: { filters: string[], indicator: string }) => {

    setDataSets([...dataSets,
      {
        indicator,
        filters
      }
    ]);
    setIndicators([...indicators, indicator]);
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

      {selectedChartType &&
      <Fragment>
        <Details summary="Chart preview" open>
          <ChartRenderer
            type={selectedChartType.type}
            indicators={indicators}
            dataSets={dataSets}
            data={Data}
            meta={Data.metaData}
            xAxis={{title: ''}}
            yAxis={{title: ''}}
          />
        </Details>
        <Details summary="Add data to chart" open>
          <ChartDataSelector
            onDataAdded={onDataAddedToChart}
            metaData={Data.metaData}
            indicatorIds={indicatorIds}
            filterIds={filterIdCombinations}
            chartType={selectedChartType}
          />
        </Details>
      </Fragment>
      }


    </div>
  );
};

export default PrototypeChartEditor;
