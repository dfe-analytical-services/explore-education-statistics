import {
  AbstractChartProps,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import React, { useEffect, useState } from 'react';

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
  width = 0,
  height = 0,
  children,
}: InfographicChartProps) => {
  const [file, setFile] = useState<string>();

  useEffect(() => {
    if (fileId && chartFileDownloadService) {
      chartFileDownloadService(data.releaseId, fileId).then(blob => {
        const a = new FileReader();
        // @ts-ignore
        a.onload = (e: ProgressEvent) => setFile(e.target.result);
        a.readAsDataURL(blob);
      });
    }
  }, [chartFileDownloadService, data.releaseId, fileId]);

  if (fileId === undefined || fileId === '') {
    return <div>Infographic not configured</div>;
  }

  return (
    <>
      {file && (
        <img
          alt="infographic"
          src={file}
          width={width > 0 ? width : undefined}
          height={height > 0 ? height : undefined}
        />
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
