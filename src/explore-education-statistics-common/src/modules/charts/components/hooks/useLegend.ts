import omit from 'lodash/omit';
import { useCallback, useState } from 'react';
import { ContentRenderer, LegendProps } from 'recharts';

export default function useLegend(): [
  LegendProps | undefined,
  ContentRenderer<LegendProps>,
] {
  const [legendProps, setLegendProps] = useState<LegendProps>();

  const renderLegend: ContentRenderer<LegendProps> = useCallback(
    nextProps => {
      const nextLegendProps = omit(nextProps, 'content');

      // Need to do a deep comparison of the props to
      // avoid falling into an infinite rendering loop.
      if (JSON.stringify(legendProps) !== JSON.stringify(nextLegendProps)) {
        setTimeout(() => {
          setLegendProps(nextLegendProps);
        });
      }

      return null;
    },
    [legendProps],
  );

  return [legendProps, renderLegend];
}
