import ExportButtonMenu from '@common/components/ExportButtonMenu';
import FigureFootnotes from '@common/components/FigureFootnotes';
import { ExportButtonContext } from '@common/contexts/ExportButtonContext';
import HorizontalBarBlock, {
  HorizontalBarProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
import InfographicBlock, {
  InfographicChartProps,
} from '@common/modules/charts/components/InfographicBlock';
import LineChartBlock, {
  LineChartProps,
} from '@common/modules/charts/components/LineChartBlock';
import MapBlock, {
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import VerticalBarBlock, {
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import React, { memo, useContext, useMemo } from 'react';

type HorizontalBarRendererProps = {
  type: 'horizontalbar';
} & HorizontalBarProps;

type InfographicRendererProps = {
  type: 'infographic';
} & InfographicChartProps;

type LineChartRendererProps = {
  type: 'line';
} & LineChartProps;

type MapBlockRendererProps = {
  type: 'map';
} & Omit<MapBlockProps, 'id'>;

type VerticalBarRendererProps = {
  type: 'verticalbar';
} & VerticalBarProps;

export type RenderableChart =
  | HorizontalBarRendererProps
  | InfographicRendererProps
  | LineChartRendererProps
  | MapBlockRendererProps
  | VerticalBarRendererProps;

export interface ChartRendererProps {
  source?: string;
  id?: string;
  chart: RenderableChart;
}

function ChartRenderer({ source, id, chart }: ChartRendererProps) {
  const { data, meta, subtitle, title, type } = chart;

  const exportRef = useContext(ExportButtonContext);

  const chartComponent = useMemo(() => {
    switch (chart.type) {
      case 'line':
        return <LineChartBlock {...chart} />;
      case 'verticalbar':
        return <VerticalBarBlock {...chart} />;
      case 'horizontalbar':
        return <HorizontalBarBlock {...chart} />;
      case 'map':
        return <MapBlock {...chart} id={`${id}-map`} />;
      case 'infographic':
        return <InfographicBlock {...chart} />;
      default:
        return <p>Unable to render invalid chart type</p>;
    }
  }, [id, chart]);

  if (data?.length > 0 && meta) {
    const footnotes = [...meta.footnotes];

    const boundaryFootnoteId = 'map-footnote';
    if (
      type === 'map' &&
      footnotes.findIndex(footnote => footnote.id === boundaryFootnoteId) === -1
    ) {
      const selectedBoundaryLevel = meta.boundaryLevels.find(
        boundaryLevel => boundaryLevel.id === chart.boundaryLevel,
      );
      if (selectedBoundaryLevel) {
        footnotes.push({
          id: boundaryFootnoteId,
          label: `This map uses the boundary data ${selectedBoundaryLevel.label}`,
        });
      }
    }

    return (
      <>
        <ExportButtonMenu />
        <figure
          ref={exportRef}
          className="govuk-!-margin-0"
          id={id}
          data-testid={id}
        >
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
      </>
    );
  }

  return <p>Unable to render chart, invalid data configured</p>;
}

export default memo(ChartRenderer);
