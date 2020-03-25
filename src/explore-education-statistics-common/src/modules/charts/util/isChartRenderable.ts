import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';

export default function isChartRenderable(
  props: ChartRendererProps | undefined,
): props is ChartRendererProps {
  if (!props) {
    return false;
  }

  if (props.type === 'infographic' && props.fileId && props.releaseId) {
    return true;
  }

  return Boolean(
    props.type &&
      props.labels &&
      props.axes?.major?.dataSets.length &&
      props.data &&
      props.meta,
  );
}
