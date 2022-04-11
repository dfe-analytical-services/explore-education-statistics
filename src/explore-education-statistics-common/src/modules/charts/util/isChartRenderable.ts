import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';

export default function isChartRenderable(
  props: ChartRendererProps | undefined,
): props is ChartRendererProps {
  if (!props) {
    return false;
  }

  if (props.type === 'infographic' && props.fileId) {
    return true;
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
