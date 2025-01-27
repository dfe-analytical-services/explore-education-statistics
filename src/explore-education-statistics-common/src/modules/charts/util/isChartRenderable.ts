import {
  DraftChartConfig,
  DraftFullChart,
  RenderableChart,
} from '../types/chart';

export default function isChartRenderable(
  draftFullChart: DraftFullChart | RenderableChart,
): draftFullChart is RenderableChart {
  const { chartConfig, data, meta } = draftFullChart;

  if (chartConfig.type === 'infographic') {
    return Boolean(chartConfig.fileId && chartConfig.fileId.length > 0);
  }

  if (chartConfig.type === 'map' && !chartConfig.boundaryLevel) {
    return false;
  }

  return Boolean(
    chartConfig.type &&
      chartConfig.axes?.major?.dataSets.length &&
      data &&
      meta,
  );
}

export function getChartPreviewText(chartConfig?: DraftChartConfig): string {
  switch (chartConfig?.type) {
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
