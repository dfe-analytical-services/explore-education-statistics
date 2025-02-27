import styles from '@admin/pages/release/datablocks/components/chart/ChartBuilder.module.scss';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ChartRenderer, {
  RenderableChart,
} from '@common/modules/charts/components/ChartRenderer';
import isChartRenderable, {
  getChartPreviewText,
} from '@common/modules/charts/util/isChartRenderable';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import React, { useRef } from 'react';

interface Props {
  chart?: RenderableChart;
  data: TableDataResult[];
  loading: boolean;
  meta: FullTableMeta;
}

const ChartBuilderPreview = ({ chart, data, loading, meta }: Props) => {
  const renderCount = useRef(0);

  return (
    <Details summary="Chart preview" open>
      <div
        className="govuk-width-container govuk-!-margin-bottom-6"
        data-testid="chartBuilderPreviewContainer"
      >
        {chart && isChartRenderable(chart) ? (
          <LoadingSpinner loading={loading} text="Loading chart data">
            <div className="govuk-width-container govuk-!-padding-4 dfe-border">
              <ChartRenderer
                chart={chart}
                data={data}
                id="chartBuilderPreview"
                key={renderCount.current}
                meta={meta}
              />
            </div>
          </LoadingSpinner>
        ) : (
          <div className={styles.previewPlaceholder}>
            <p>{getChartPreviewText(chart)}</p>
          </div>
        )}
      </div>
    </Details>
  );
};

export default ChartBuilderPreview;
