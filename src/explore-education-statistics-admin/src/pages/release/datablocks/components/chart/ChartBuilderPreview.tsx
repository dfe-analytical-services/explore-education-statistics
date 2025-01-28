import styles from '@admin/pages/release/datablocks/components/chart/ChartBuilder.module.scss';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ChartRenderer from '@common/modules/charts/components/ChartRenderer';
import { DraftFullChart } from '@common/modules/charts/types/chart';
import isChartRenderable, {
  getChartPreviewText,
} from '@common/modules/charts/util/isChartRenderable';
import React, { useRef } from 'react';

interface Props {
  draftFullChart: DraftFullChart;
  loading: boolean;
}

const ChartBuilderPreview = ({ draftFullChart, loading }: Props) => {
  const renderCount = useRef(0);

  return (
    <Details summary="Chart preview" open>
      <div
        className="govuk-width-container govuk-!-margin-bottom-6"
        data-testid="chartBuilderPreviewContainer"
      >
        {isChartRenderable(draftFullChart) ? (
          <LoadingSpinner loading={loading} text="Loading chart data">
            <div className="govuk-width-container govuk-!-padding-4 dfe-border">
              <ChartRenderer
                fullChart={draftFullChart}
                key={renderCount.current}
                id="chartBuilderPreview"
              />
            </div>
          </LoadingSpinner>
        ) : (
          <div className={styles.previewPlaceholder}>
            <p>{getChartPreviewText(draftFullChart.chartConfig)}</p>
          </div>
        )}
      </div>
    </Details>
  );
};

export default ChartBuilderPreview;
