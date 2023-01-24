import CollapsibleList from '@common/components/CollapsibleList';
import SummaryListItem from '@common/components/SummaryListItem';
import { GroupedFilterOptions } from '@common/services/tableBuilderService';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { Path, Control, useWatch } from 'react-hook-form';

type RHFSelectedFiltersProps<TFormValues> = {
  control: Control<TFormValues>;
  filterGroup: {
    legend: string;
    hint?: string | undefined;
    id: string;
    options: GroupedFilterOptions;
    order: number;
    totalValue?: string | undefined;
    name: string;
  };
  name: Path<TFormValues>;
};

const RHFSelectedFilters = <TFormValues extends Record<string, unknown>>({
  control,
  filterGroup,
  name,
}: RHFSelectedFiltersProps<TFormValues>) => {
  const values = useWatch({ control, name }) as string[];

  if (!values) {
    return null;
  }

  return (
    <SummaryListItem key={name} term={filterGroup.legend}>
      <CollapsibleList
        id={`filtersList-${name}`}
        itemName="filter"
        itemNamePlural="filters"
      >
        {orderBy(Object.values(filterGroup.options), 'order')
          .flatMap(group => group.options)
          .filter(option => values.includes(option.value))
          .map(option => (
            <li key={option.value}>{option.label}</li>
          ))}
      </CollapsibleList>
    </SummaryListItem>
  );
};

export default RHFSelectedFilters;
