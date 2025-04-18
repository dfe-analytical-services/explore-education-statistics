import ChartExportMenu from '@common/modules/charts/components/ChartExportMenu';
import FigureFootnotes from '@common/components/FigureFootnotes';
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
import React, { memo, useMemo, useRef, useState } from 'react';
import getMapInitialBoundaryLevel from './utils/getMapInitialBoundaryLevel';

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
  showExportMenu?: boolean;
}

function ChartRenderer({
  source,
  id,
  chart,
  showExportMenu = true,
}: ChartRendererProps) {
  const { data, meta, subtitle, title, type } = chart;
  const [selectedBoundaryLevelId, setSelectedBoundaryLevelId] = useState(
    type === 'map' ? getMapInitialBoundaryLevel(chart) : undefined,
  );

  const chartRef = useRef<HTMLElement>(null);

  const chartComponent = useMemo(() => {
    switch (type) {
      case 'line':
        return <LineChartBlock {...chart} />;
      case 'verticalbar':
        return <VerticalBarBlock {...chart} />;
      case 'horizontalbar':
        return <HorizontalBarBlock {...chart} />;
      case 'map':
        return (
          <MapBlock
            {...chart}
            onBoundaryLevelChange={number => {
              setSelectedBoundaryLevelId(number);
              return chart.onBoundaryLevelChange(number);
            }}
            id={`${id}-map`}
          />
        );
      case 'infographic':
        return <InfographicBlock {...chart} />;
      default:
        return <p>Unable to render invalid chart type</p>;
    }
  }, [type, chart, id]);

  const footnotes = useMemo(() => {
    if (!data?.length || !meta) {
      return [];
    }

    const metaFootnotes = [...meta.footnotes];
    const boundaryFootnoteId = 'map-footnote';

    if (
      type === 'map' &&
      metaFootnotes.findIndex(
        footnote => footnote.id === boundaryFootnoteId,
      ) === -1
    ) {
      const selectedBoundaryLevel = meta.boundaryLevels.find(
        boundaryLevel => boundaryLevel.id === selectedBoundaryLevelId,
      );
      if (selectedBoundaryLevel) {
        metaFootnotes.push({
          id: boundaryFootnoteId,
          label: `This map uses the boundary data ${selectedBoundaryLevel.label}`,
        });
      }
    }

    return metaFootnotes;
  }, [data?.length, meta, selectedBoundaryLevelId, type]);

  if (data?.length && meta) {
    return (
      <>
        {showExportMenu && (
          <ChartExportMenu chartRef={chartRef} chartTitle={title} />
        )}
        <figure
          ref={chartRef}
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
