import {
  AbstractChartProps,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import React, { useEffect, useState } from 'react';

export type GetInfographic = (
  releaseId: string,
  fileName: string,
) => Promise<Blob>;

export interface InfographicChartProps extends AbstractChartProps {
  releaseId?: string;
  fileId?: string;
  getInfographic?: GetInfographic;
}

const Infographic = ({
  releaseId,
  fileId,
  getInfographic,
  width = 0,
  height = 0,
}: InfographicChartProps) => {
  const [file, setFile] = useState<string>();

  useEffect(() => {
    if (fileId && releaseId && getInfographic) {
      getInfographic(releaseId, fileId).then(blob => {
        const a = new FileReader();
        // @ts-ignore
        a.onload = (e: ProgressEvent) => setFile(e.target.result);
        a.readAsDataURL(blob);
      });
    }
  }, [getInfographic, fileId, releaseId]);

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
