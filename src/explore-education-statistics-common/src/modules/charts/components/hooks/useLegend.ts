import omit from 'lodash/omit';
import { useCallback, useState } from 'react';
import { LegendProps, DefaultLegendContentProps } from 'recharts';
import { ContentType } from 'recharts/types/component/DefaultLegendContent';

export default function useLegend(): [LegendProps | undefined, ContentType] {
  const [legendProps, setLegendProps] = useState<LegendProps>();

  const renderLegend: ContentType = useCallback(
    (nextProps: DefaultLegendContentProps) => {
      const nextLegendProps = omit(nextProps, 'content');

      // Need to do a deep comparison of the props to
      // avoid falling into an infinite rendering loop.
      if (JSON.stringify(legendProps) !== JSON.stringify(nextLegendProps)) {
        setTimeout(() => {
          setLegendProps(nextLegendProps as LegendProps);
        });
      }

      return null;
    },
    [legendProps],
  );

  return [legendProps, renderLegend];
}
