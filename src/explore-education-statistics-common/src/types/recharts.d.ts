/* eslint-disable import/prefer-default-export */
/* eslint-disable react/prefer-stateless-function */
/* eslint-disable max-classes-per-file */

declare module 'recharts/lib/component/DefaultLegendContent';

declare module 'recharts' {
  import { LineProps, LabelProps } from 'recharts/types/index';
  import { CategoricalChartProps } from 'recharts/types/chart/generateCategoricalChart';
  import { Props as SymbolProps } from 'recharts/types/shape/Symbols';

  export class LineChart extends React.Component<
    CategoricalChartProps & {
      role?: string;
      focusable?: boolean;
    }
  > {}

  export class Line extends React.Component<
    LineProps & {
      label?: (props: LabelProps & { index: number }) => React.ReactNode;
      dot?: (props: SymbolProps) => React.ReactNode;
    }
  > {}

  export class BarChart extends React.Component<
    CategoricalChartProps & {
      role?: string;
      focusable?: boolean;
    }
  > {}

  export * from 'recharts/';
}
