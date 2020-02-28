import {
  AbstractChartProps,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import * as React from 'react';

export interface InfographicChartProps extends AbstractChartProps {
  fileId?: string;

  chartFileDownloadService?: (
    releaseId: string,
    fileName: string,
  ) => Promise<Blob>;
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
      chartFileDownloadService(data.releaseId, fileId).then(blob => {
        const a = new FileReader();
        // @ts-ignore
        a.onload = (e: ProgressEvent) => setFile(e.target.result);
        a.readAsDataURL(blob);
      });
    }
  }, [chartFileDownloadService, data.releaseId, fileId]);

  if (fileId === undefined || fileId === '')
    return <div>Infographic not configured</div>;

  return (
    <>
      {file && (
        <img alt="infographic" src={file} width={width} height={height} />
      )}
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

  requiresGeoJson: false,
};

Infographic.definition = definition;

export default Infographic;
