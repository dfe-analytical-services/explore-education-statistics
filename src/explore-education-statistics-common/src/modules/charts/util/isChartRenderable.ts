import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';

export default function isChartRenderable(props: ChartRendererProps): boolean {
  if (props.type === 'infographic') {
    return props.fileId.length > 0;
  }

  if (props.type === 'map' && !props.boundaryLevel) {
    return false;
  }

  return Boolean(
    props.type &&
      props.axes?.major?.dataSets.length &&
      props.data &&
      props.meta,
  );
}

export function getChartPreviewText(
  props: ChartRendererProps | undefined,
): string | undefined {
  return props?.type === 'map'
    ? 'Add data sets and choose a version of geographic data to view a preview of the chart'
    : 'Add data to view a preview of the chart';
}
