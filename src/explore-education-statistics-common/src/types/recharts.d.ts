/* eslint-disable import/no-duplicates,react/prefer-stateless-function */

declare module 'recharts' {
  import * as React from 'react';

  export class LineChart extends React.Component<
    LineChartProps & {
      role?: string;
      focusable?: boolean;
    }
  > {}

  export class BarChart extends React.Component<
    BarChartProps & {
      role?: string;
      focusable?: boolean;
    }
  > {}

  export * from 'recharts/index';
}

declare module 'recharts/lib/component/DefaultLegendContent' {
  import { ComponentType } from 'react';
  import { LegendProps } from 'recharts';

  const DefaultLegendContent: ComponentType<LegendProps>;

  export default DefaultLegendContent;
}
