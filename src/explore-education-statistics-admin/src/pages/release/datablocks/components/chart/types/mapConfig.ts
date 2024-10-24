import { DataGroupingConfig } from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';

export interface MapBoundaryLevelConfig {
  boundaryLevel?: number;
  dataSetConfigs: {
    boundaryLevel?: number;
    dataSet: DataSet;
  }[];
}

export interface MapDataGroupingConfig {
  dataSetConfigs: {
    dataGrouping: DataGroupingConfig;
    dataSet: DataSet;
  }[];
}
