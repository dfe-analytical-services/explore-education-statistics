import styles from '@admin/pages/release/datablocks/components/ChartBuilder.module.scss';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useDebouncedEffect from '@common/hooks/useDebouncedEffect';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import {
  AxesConfiguration,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import isChartRenderable from '@common/modules/charts/util/isChartRenderable';
import React, { useRef, useState } from 'react';

interface Props {
  axes: AxesConfiguration;
  chart?: ChartRendererProps;
  definition: ChartDefinition;
  loading: boolean;
}

const ChartBuilderPreview = ({ axes, chart, definition, loading }: Props) => {
  const renderCount = useRef(0);

  const [currentChart, setCurrentChart] = useState<
    ChartRendererProps | undefined
  >(chart);

  useDebouncedEffect(
    () => {
      // We need to force re-rendering as the chart
      // won't always resize correctly, causing
      // things like labels to overlap with the axes.
      renderCount.current += 1;

      setCurrentChart(chart);
    },
    300,
    [chart],
  );

  return (
    <Details summary="Chart preview" open>
      <div
        className="govuk-width-container govuk-!-margin-bottom-6"
        id="chartBuilderPreviewContainer"
        data-render-count={renderCount.current}
      >
        {isChartRenderable(currentChart) ? (
          <LoadingSpinner loading={loading} text="Loading chart data">
            <ChartRenderer
              {...currentChart}
              key={renderCount.current}
              id="chartBuilderPreview"
            />
          </LoadingSpinner>
        ) : (
          <div className={styles.previewPlaceholder}>
            {Object.keys(axes).length > 0 ? (
              <p>Add data to view a preview of the chart</p>
            ) : (
              <p>Configure the {definition.name} to view a preview</p>
            )}
          </div>
        )}
      </div>
    </Details>
  );
};

export default ChartBuilderPreview;
