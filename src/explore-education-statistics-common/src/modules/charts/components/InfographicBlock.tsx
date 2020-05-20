import LoadingSpinner from '@common/components/LoadingSpinner';
import ResponsiveImage from '@common/components/ResponsiveImage';
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
  alt,
  fileId,
  getInfographic,
  width,
  height,
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
        <ResponsiveImage
          alt={alt}
          src={file}
          height={height ? `${height}px` : undefined}
          width={width ? `${width}px` : undefined}
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
    canSort: false,
    fixedAxisGroupBy: true,
    hasReferenceLines: false,
    hasLegend: false,
    requiresGeoJson: false,
  },
  data: [],
  axes: {},
};

export default memo(InfographicBlock);
