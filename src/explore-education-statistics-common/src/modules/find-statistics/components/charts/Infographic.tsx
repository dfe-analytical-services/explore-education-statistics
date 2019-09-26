import {
  AbstractChartProps,
  ChartDefinition,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import * as React from 'react';

export interface InfographicChartProps extends AbstractChartProps {
  fileId?: string;

  chartFileDownloadService?: (releaseId: string, fileName: string) => string;
}

const Infographic = ({
  data,
  fileId,
  chartFileDownloadService,
  width,
  height,
  children,
}: InfographicChartProps) => {
  const [file, setFile] = React.useState<string>();

  React.useEffect(() => {
    if (fileId && chartFileDownloadService) {
      setFile(chartFileDownloadService(data.releaseId, fileId));
    }
  }, [chartFileDownloadService, data.releaseId, fileId]);

  if (fileId === undefined || fileId === '')
    return <div>Infographic not configured</div>;

  return (
    <>
      <img alt="infographic" src={file} width={width} height={height} />
      {children}
    </>
  );
};

const definition: ChartDefinition = {
  type: 'infographic',
  name: 'Infographic',

  height: 600,

  capabilities: {
    dataSymbols: false,
    stackable: false,
    lineStyle: false,
    gridLines: false,
    canSize: true,
    fixedAxisGroupBy: true,
    hasAxes: false,
    hasReferenceLines: false,
    hasLegend: false,
  },

  data: [],

  axes: [],
};

Infographic.definition = definition;

export default Infographic;
