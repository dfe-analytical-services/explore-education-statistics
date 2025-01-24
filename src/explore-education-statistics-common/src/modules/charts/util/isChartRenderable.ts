import { RenderableChart } from '@common/modules/charts/components/ChartRenderer';

export default function isChartRenderable(props: RenderableChart): boolean {
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

export function getChartPreviewText(props?: RenderableChart): string {
  switch (props?.type) {
    case undefined:
      return '';
    case 'map':
      return 'Add data and choose a version of geographic data to view a preview';
    case 'infographic':
      return 'Choose an infographic file to view a preview';
    default:
      return 'Configure the chart and add data to view a preview';
  }
}
