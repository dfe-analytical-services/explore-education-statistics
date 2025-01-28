import {
  DraftChartConfig,
  DraftFullChart,
  MapChartConfig,
  RenderableChart,
} from '../types/chart';

function isMapRenderable(
  draftMapChartConfig: DraftChartConfig<MapChartConfig>,
): boolean {
  if (Number.isNaN(draftMapChartConfig.boundaryLevel)) {
    return false;
  }
  return true;
}

export default function isChartRenderable(
  draftFullChart: DraftFullChart,
): draftFullChart is RenderableChart {
  const { chartConfig, data, meta } = draftFullChart;

  if (chartConfig.type === 'infographic') {
    return Boolean(chartConfig.fileId && chartConfig.fileId.length > 0);
  }

  if (chartConfig.type === 'map') {
    return isMapRenderable(chartConfig);
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
