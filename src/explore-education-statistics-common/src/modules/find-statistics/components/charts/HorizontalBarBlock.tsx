import {
  AbstractChart,
  ChartProps,
} from '@common/modules/find-statistics/components/charts/AbstractChart';
import React from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
} from 'recharts';

import { colours } from './Charts';

interface StackedBarHorizontalProps extends ChartProps {
  stacked?: boolean;
}

export default class HorizontalBarBlock extends AbstractChart<
  StackedBarHorizontalProps
> {
  public render() {
    const chartData = this.props.characteristicsData.result.map(data => {
      return data.indicators;
    });

    return (
      <ResponsiveContainer width="100%" height={this.props.height || 600}>
        <BarChart
          data={chartData}
          layout="vertical"
          margin={this.calculateMargins()}
        >
          {this.calculateYAxis({
            type: 'category',
            dataKey: this.props.yAxis.key || 'name',
          })}

          <CartesianGrid />

          {this.calculateXAxis({ type: 'number' })}

          <Tooltip cursor={false} />
          <Legend />

          {this.props.chartDataKeys.map((dataKey, index) => {
            const key = index;
            return (
              <Bar
                key={key}
                dataKey={dataKey}
                name={this.props.labels[dataKey]}
                fill={colours[index]}
                stackId={this.props.stacked ? 'a' : undefined}
              />
            )
          })}

          {this.generateReferenceLines()}
        </BarChart>
      </ResponsiveContainer>
    );
  }
}
