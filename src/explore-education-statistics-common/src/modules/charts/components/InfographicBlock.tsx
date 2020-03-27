import {
  ChartDefinition,
  ChartProps,
} from '@common/modules/charts/types/chart';
import React, { memo, useEffect, useState } from 'react';

export type GetInfographic = (fileId: string) => Promise<Blob>;

export interface InfographicChartProps extends ChartProps {
  fileId: string;
  getInfographic?: GetInfographic;
}

const InfographicBlock = ({
  fileId,
  getInfographic,
  width = 0,
  height = 0,
}: InfographicChartProps) => {
  const [file, setFile] = useState<string>();

  useEffect(() => {
    if (fileId && getInfographic) {
      getInfographic(fileId).then(blob => {
        const a = new FileReader();
        // @ts-ignore
        a.onload = (e: ProgressEvent) => setFile(e.target.result);
        a.readAsDataURL(blob);
      });
    }
  }, [getInfographic, fileId]);

  if (!fileId || !file) {
    return null;
  }

  return (
    <img
      alt="infographic"
      src={file}
      width={width > 0 ? width : undefined}
      height={height > 0 ? height : undefined}
    />
  );
};

export const infographicBlockDefinition: ChartDefinition = {
  type: 'infographic',
  name: 'Infographic',
  options: {
    defaults: {
      height: 600,
      legend: 'none',
    },
  },
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
    requiresGeoJson: false,
  },
  data: [],
  axes: {},
};

export default memo(InfographicBlock);
