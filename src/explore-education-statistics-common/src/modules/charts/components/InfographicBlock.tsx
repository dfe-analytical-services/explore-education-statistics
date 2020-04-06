import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import {
  ChartDefinition,
  ChartProps,
} from '@common/modules/charts/types/chart';
import toDataUrl from '@common/utils/file/toDataUrl';
import React, { memo } from 'react';

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
  const { value: file, error, isLoading } = useAsyncRetry(async () => {
    if (fileId && getInfographic) {
      const infographic = await getInfographic(fileId);
      const dataUrl = await toDataUrl(infographic);
      return dataUrl.toString();
    }

    return undefined;
  }, [getInfographic, fileId]);

  if (error) {
    return <p className="govuk-inset-text">Could not load infographic</p>;
  }

  if (!fileId) {
    return null;
  }

  return (
    <LoadingSpinner loading={isLoading}>
      {file && (
        <img
          alt="infographic"
          src={file}
          width={width > 0 ? width : undefined}
          height={height > 0 ? height : undefined}
        />
      )}
    </LoadingSpinner>
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
