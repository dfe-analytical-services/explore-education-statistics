import omit from 'lodash/omit';
import { useCallback, useState } from 'react';
import { LegendProps, DefaultLegendContentProps } from 'recharts';
import { ContentType } from 'recharts/types/component/DefaultLegendContent';

export default function useLegend(): [
  DefaultLegendContentProps | undefined,
  ContentType,
] {
  const [legendProps, setLegendProps] = useState<DefaultLegendContentProps>();

  const renderLegend: ContentType = useCallback(
    (nextProps: DefaultLegendContentProps) => {
      const nextLegendProps: LegendProps = {
        ...omit(nextProps, 'content'),
        width: nextProps.width ? Number(nextProps.width) : undefined,
        height: nextProps.height ? Number(nextProps.height) : undefined,
      };

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
