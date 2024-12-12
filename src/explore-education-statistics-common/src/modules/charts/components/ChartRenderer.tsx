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
import React, { memo, useMemo } from 'react';

export type ChartRendererProps = {
  source?: string;
} & (
  | ({
      type: 'map';
    } & Omit<MapBlockProps, 'id'>)
  | ({
      type: 'line';
    } & LineChartProps)
  | ({
      type: 'verticalbar';
    } & VerticalBarProps)
  | ({
      type: 'horizontalbar';
    } & HorizontalBarProps)
  | ({
      type: 'infographic';
    } & InfographicChartProps)
);

export interface ChartRendererInternalProps {
  id: string;
}

function ChartRenderer({
  source,
  id,
  ...props
}: ChartRendererProps & ChartRendererInternalProps) {
  const { data, meta, subtitle, title, type } = props;

  const chart = useMemo(() => {
    switch (props.type) {
      case 'line':
        return <LineChartBlock {...props} />;
      case 'verticalbar':
        return <VerticalBarBlock {...props} />;
      case 'horizontalbar':
        return <HorizontalBarBlock {...props} />;
      case 'map':
        return <MapBlock {...props} id={`${id}-map`} />;
      case 'infographic':
        return <InfographicBlock {...props} />;
      default:
        return <p>Unable to render invalid chart type</p>;
    }
  }, [id, props]);

  if (data?.length > 0 && meta) {
    const footnotes = [...meta.footnotes];

    const boundaryFootnoteId = 'map-footnote';
    if (
      type === 'map' &&
      footnotes.findIndex(footnote => footnote.id === boundaryFootnoteId) === -1
    ) {
      const selectedBoundaryLevel = meta.boundaryLevels.find(
        boundaryLevel => boundaryLevel.id === props.boundaryLevel,
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

        {chart}

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
