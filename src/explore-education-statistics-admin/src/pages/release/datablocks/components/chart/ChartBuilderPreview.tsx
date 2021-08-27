import styles from '@admin/pages/release/datablocks/components/chart/ChartBuilder.module.scss';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ChartRenderer, {
  ChartRendererProps,
} from '@common/modules/charts/components/ChartRenderer';
import {
  AxesConfiguration,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import isChartRenderable from '@common/modules/charts/util/isChartRenderable';
import React, { useRef } from 'react';

interface Props {
  axes: AxesConfiguration;
  chart?: ChartRendererProps;
  definition: ChartDefinition;
  loading: boolean;
}

const ChartBuilderPreview = ({ axes, chart, definition, loading }: Props) => {
  const renderCount = useRef(0);

  return (
    <Details summary="Chart preview" open>
      <div
        className="govuk-width-container govuk-!-margin-bottom-6"
        id="chartBuilderPreviewContainer"
      >
        {isChartRenderable(chart) ? (
          <LoadingSpinner loading={loading} text="Loading chart data">
            <ChartRenderer
              {...chart}
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
