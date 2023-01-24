import CollapsibleList from '@common/components/CollapsibleList';
import SummaryListItem from '@common/components/SummaryListItem';
import { IndicatorOption } from '@common/services/tableBuilderService';
import React from 'react';
import { Path, Control, useWatch } from 'react-hook-form';

type RHFSelectedIndicatorsProps<TFormValues> = {
  name: Path<TFormValues>;
  control: Control<TFormValues>;
  indicators: {
    id: string;
    label: string;
    options: IndicatorOption[];
    order: number;
  }[];
};

const RHFSelectedIndicators = <TFormValues extends Record<string, unknown>>({
  name,
  control,
  indicators,
}: RHFSelectedIndicatorsProps<TFormValues>) => {
  const values = useWatch({ control, name }) as string[];

  return (
    <SummaryListItem term="Indicators">
      <CollapsibleList
        id="indicatorsList"
        itemName="indicator"
        itemNamePlural="indicators"
      >
        {indicators
          .flatMap(group => group.options)
          .filter(indicator => values.includes(indicator.value))
          .map(indicator => (
            <li key={indicator.value}>{indicator.label}</li>
          ))}
      </CollapsibleList>
    </SummaryListItem>
  );
};

export default RHFSelectedIndicators;
