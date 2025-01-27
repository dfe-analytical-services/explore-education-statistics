import FigureFootnotes from '@common/components/FigureFootnotes';
import HorizontalBarBlock from '@common/modules/charts/components/HorizontalBarBlock';
import InfographicBlock from '@common/modules/charts/components/InfographicBlock';
import LineChartBlock from '@common/modules/charts/components/LineChartBlock';
import MapBlock, {
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import VerticalBarBlock from '@common/modules/charts/components/VerticalBarBlock';
import { RenderableChart } from '@common/modules/charts/types/chart';
import React, { memo, useMemo } from 'react';

export interface ChartRendererProps {
  source?: string;
  id?: string;
  fullChart: RenderableChart;
}

function ChartRenderer({ source, id, fullChart }: ChartRendererProps) {
  const { data, meta, chartConfig } = fullChart;
  const { subtitle, title } = chartConfig;

  const chartComponent = useMemo(() => {
    switch (chartConfig.type) {
      case 'line':
        return (
          <LineChartBlock data={data} meta={meta} chartConfig={chartConfig} />
        );
      case 'verticalbar':
        return (
          <VerticalBarBlock data={data} meta={meta} chartConfig={chartConfig} />
        );
      case 'horizontalbar':
        return (
          <HorizontalBarBlock
            data={data}
            meta={meta}
            chartConfig={chartConfig}
          />
        );
      case 'map':
        return (
          <MapBlock
            {...(fullChart as Omit<MapBlockProps, 'id'>)}
            chartConfig={chartConfig}
            id={`${id}-map`}
          />
        );
      case 'infographic':
        return (
          <InfographicBlock data={data} meta={meta} chartConfig={chartConfig} />
        );
      default:
        return <p>Unable to render invalid chart type</p>;
    }
  }, [chartConfig, data, meta, fullChart, id]);

  if (data?.length > 0 && meta) {
    const footnotes = [...meta.footnotes];

    const boundaryFootnoteId = 'map-footnote';
    if (
      chartConfig.type === 'map' &&
      footnotes.findIndex(footnote => footnote.id === boundaryFootnoteId) === -1
    ) {
      const selectedBoundaryLevel = meta.boundaryLevels.find(
        boundaryLevel => boundaryLevel.id === chartConfig.boundaryLevel,
      );
      if (selectedBoundaryLevel) {
        footnotes.push({
          id: boundaryFootnoteId,
          label: `This map uses the boundary data ${selectedBoundaryLevel.label}`,
        });
      }
    }

    return (
      <figure className="govuk-!-margin-0" id={id} data-testid={id}>
        {title && (
          <figcaption>
            <p
              className="govuk-heading-s govuk-!-margin-bottom-1"
              data-testid="chart-title"
            >
              {title}
            </p>
            {subtitle && <p data-testid="chart-subtitle">{subtitle}</p>}
          </figcaption>
        )}

        {chartComponent}

        <FigureFootnotes
          footnotes={footnotes}
          headingHiddenText={`for ${title}`}
          id={`chartFootnotes-${id}`}
        />

        {source && <p className="govuk-body-s">Source: {source}</p>}
      </figure>
    );
  }

  return <p>Unable to render chart, invalid data configured</p>;
}

export default memo(ChartRenderer);
