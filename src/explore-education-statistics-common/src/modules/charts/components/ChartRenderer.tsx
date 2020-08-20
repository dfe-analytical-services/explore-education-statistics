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
import MapBlock from '@common/modules/charts/components/MapBlock';
import { MapBlockProps } from '@common/modules/charts/components/MapBlockInternal';
import VerticalBarBlock, {
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import React, { memo, useMemo } from 'react';

export type ChartRendererProps = {
  source?: string;
} & (
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
      type: 'map';
    } & Omit<MapBlockProps, 'id'>)
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
  const { data, meta, title } = props;

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

  if (data && meta && data.length > 0) {
    return (
      <figure className="govuk-!-margin-0" id={id}>
        {title && <figcaption className="govuk-heading-s">{title}</figcaption>}

        {chart}

        <FigureFootnotes footnotes={meta.footnotes} />

        {source && <p className="govuk-body-s">Source: {source}</p>}
      </figure>
    );
  }

  return <p>Unable to render chart, invalid data configured</p>;
}

export default memo(ChartRenderer);
