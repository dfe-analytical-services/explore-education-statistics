import {
  DataBlockResponse,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import { ChartDataSet } from '@common/services/publicationService';
import * as React from 'react';

interface Props {
  dataSets: ChartDataSet[];
  data: DataBlockResponse;
  meta: DataBlockMetadata;
}

const ChartConfiguration = ({ dataSets, data, meta }: Props) => {
  return <div />;
};

export default ChartConfiguration;
