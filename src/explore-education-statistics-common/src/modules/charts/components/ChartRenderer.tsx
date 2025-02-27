import FigureFootnotes from '@common/components/FigureFootnotes';
import HorizontalBarBlock, {
  HorizontalBarBlockProps,
} from '@common/modules/charts/components/HorizontalBarBlock';
import InfographicBlock, {
  InfographicBlockProps,
} from '@common/modules/charts/components/InfographicBlock';
import LineChartBlock, {
  LineChartBlockProps,
} from '@common/modules/charts/components/LineChartBlock';
import MapBlock, {
  MapBlockProps,
} from '@common/modules/charts/components/MapBlock';
import VerticalBarBlock, {
  VerticalBarBlockProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import React, { memo, useMemo, useState } from 'react';
import getMapInitialBoundaryLevel from './utils/getMapInitialBoundaryLevel';

export type RenderableChart =
  | HorizontalBarBlockProps
  | InfographicBlockProps
  | LineChartBlockProps
  | MapBlockProps
  | VerticalBarBlockProps;

export interface ChartRendererProps {
  chart: RenderableChart;
  id?: string;
  source?: string;
}

function ChartRenderer({ chart, id, source }: ChartRendererProps) {
  const { subtitle, title, type } = chart;
  const [selectedBoundaryLevelId, setSelectedBoundaryLevelId] = useState(
    type === 'map' ? getMapInitialBoundaryLevel(chart) : undefined,
  );

  const chartComponent = useMemo(() => {
    switch (chart.type) {
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
  }, [chart, id]);

  const footnotes = useMemo(() => {
    if (!('data' in chart) || !chart.data?.length || !('meta' in chart)) {
      return [];
    }

    const metaFootnotes = [...chart.meta.footnotes];
    const boundaryFootnoteId = 'map-footnote';

    if (
      type === 'map' &&
      metaFootnotes.findIndex(
        footnote => footnote.id === boundaryFootnoteId,
      ) === -1
    ) {
      const selectedBoundaryLevel = chart.meta.boundaryLevels.find(
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
  }, [chart, selectedBoundaryLevelId, type]);

  if ('data' in chart && chart.data?.length && 'meta' in chart && chart.meta) {
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
