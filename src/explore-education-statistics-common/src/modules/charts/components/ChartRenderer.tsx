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
import { MapBlockInternalProps } from '@common/modules/charts/components/MapBlockInternal';
import VerticalBarBlock, {
  VerticalBarProps,
} from '@common/modules/charts/components/VerticalBarBlock';
import { chartDefinitions } from '@common/modules/charts/types/chart';
import omit from 'lodash/omit';
import React, { memo, useMemo, useState } from 'react';
import { ContentRenderer, LegendProps } from 'recharts';
import DefaultLegendContent from 'recharts/lib/component/DefaultLegendContent';

function hasLegend(props: ChartRendererProps): boolean {
  return chartDefinitions.some(
    chart => chart.type === props.type && chart.capabilities.hasLegend,
  );
}

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
    } & MapBlockInternalProps)
  | ({
      type: 'infographic';
    } & InfographicChartProps)
);

function ChartRenderer({ title, source, ...props }: ChartRendererProps) {
  const { data, meta } = props;

  const [legendProps, setLegendProps] = useState<LegendProps>();

  const renderLegend: ContentRenderer<LegendProps> = useMemo(
    () => nextProps => {
      const nextLegendProps = omit(nextProps, 'content');

      // Need to do a deep comparison of the props to
      // avoid falling into an infinite rendering loop.
      if (JSON.stringify(legendProps) !== JSON.stringify(nextLegendProps)) {
        setLegendProps(nextLegendProps);
      }

      return null;
    },
    [legendProps],
  );

  const chart = useMemo(() => {
    switch (props.type) {
      case 'line':
        return <LineChartBlock {...props} renderLegend={renderLegend} />;
      case 'verticalbar':
        return <VerticalBarBlock {...props} renderLegend={renderLegend} />;
      case 'horizontalbar':
        return <HorizontalBarBlock {...props} renderLegend={renderLegend} />;
      case 'map':
        return <MapBlock {...props} />;
      case 'infographic':
        return <InfographicBlock {...props} />;
      default:
        return <p>Unable to render invalid chart type</p>;
    }
  }, [props, renderLegend]);

  // TODO : Temporary sort on the results to get them in date order
  // data.result.sort((a, b) => a.timePeriod.localeCompare(b.timePeriod));

  if (data && meta && data.result.length > 0) {
    return (
      <figure className="govuk-!-margin-0">
        {title && <figcaption className="govuk-heading-s">{title}</figcaption>}

        {hasLegend(props) && props.legend === 'top' && legendProps && (
          <div className="govuk-!-margin-bottom-6">
            <DefaultLegendContent {...legendProps} />
          </div>
        )}

        <div className="govuk-!-margin-bottom-6">{chart}</div>

        {hasLegend(props) && props.legend === 'bottom' && legendProps && (
          <div className="govuk-!-margin-bottom-6">
            <DefaultLegendContent {...legendProps} />
          </div>
        )}

        <FigureFootnotes footnotes={meta.footnotes} />

        {source && <p className="govuk-body-s">Source: {source}</p>}
      </figure>
    );
  }

  return <p>Unable to render chart, invalid data configured</p>;
}

export default memo(ChartRenderer);
