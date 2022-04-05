import ReorderList, {
  FormattedIndicators,
  ReorderProps,
} from '@admin/pages/release/data/components/ReorderList';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import tableBuilderService, {
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';

// EES-1243 - remove this when the backend is done
const testSubjectMeta: SubjectMeta = {
  filters: {},
  indicators: {
    Category1: {
      id: 'category-1-id',
      label: 'Category 1',
      options: [
        {
          value: 'category-1-item-1',
          label: 'Category 1 Item 1',
          unit: '',
          name: 'category_1_item_1',
        },
        {
          value: 'category-1-item-2',
          label: 'Category 1 Item 2',
          unit: '',
          name: 'category_1_item_2',
        },
        {
          value: 'category-1-item-3',
          label: 'Category 1 Item 3',
          unit: '',
          name: 'category_1_item_3',
        },
      ],
      order: 1,
    },
    Category2: {
      id: 'category-2-id',
      label: 'Category 2',
      options: [
        {
          value: 'category-2-item-1',
          label: 'Category 2 Item 1',
          unit: '',
          name: 'category_2_item_1',
        },
      ],
      order: 0,
    },
  },
  locations: {},
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
};

interface UpdatedIndicator {
  id?: string;
  indicatorItems: string[];
}
export type UpdateIndicatorsRequest = UpdatedIndicator[];

interface Props {
  subject: Subject;
  onCancel: () => void;
  onSave: (orderedIndicators: UpdateIndicatorsRequest) => void;
}

const ReorderIndicatorsList = ({ subject, onCancel, onSave }: Props) => {
  const { value: subjectMeta, isLoading } = useAsyncHandledRetry(
    () => tableBuilderService.getSubjectMeta(subject.id),
    [subject.id],
  );

  const [indicators, setIndicators] = useState<FormattedIndicators[]>([]);

  useEffect(() => {
    if (isLoading) {
      return;
    }
    // EES-1243 - restore this line to use the real indicators
    // const indicatorsMeta = subjectMeta?.indicators || {};
    const indicatorsMeta = testSubjectMeta.indicators;

    // Transforming the indicators to be nested arrays rather than keyed objects.
    // Order by the order field.
    const formattedIndicators = orderBy(
      Object.entries(indicatorsMeta).map(([, item]) => {
        return {
          id: item.id,
          label: item.label,
          order: item.order,
          items: item.options.map(option => ({
            id: option.value,
            label: option.label,
          })),
        };
      }),
      'order',
    );

    setIndicators(formattedIndicators);
  }, [isLoading, subjectMeta?.indicators]);

  const handleReorder = ({ reordered, parentCategoryId }: ReorderProps) => {
    // reordering indicators
    if (!parentCategoryId) {
      setIndicators(reordered as FormattedIndicators[]);
      return;
    }
    // reordering indicator items
    const reorderedIndicators = indicators.map(indicator =>
      indicator.id !== parentCategoryId
        ? indicator
        : { ...indicator, items: reordered },
    ) as FormattedIndicators[];

    setIndicators(reorderedIndicators);
  };

  const handleSave = () => {
    const updateIndicatorsRequest: UpdateIndicatorsRequest = indicators.map(
      indicator => {
        return {
          id: indicator.id,
          indicatorItems: indicator.items.map(item => item.id),
        };
      },
    );
    onSave(updateIndicatorsRequest);
  };

  return (
    <>
      <h3>Reorder indicators for {subject.name}</h3>
      <LoadingSpinner loading={isLoading}>
        {indicators.length === 0 ? (
          <p>No indicators available.</p>
        ) : (
          <ReorderList listItems={indicators} onReorder={handleReorder} />
        )}
        <ButtonGroup>
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
          {indicators.length > 0 && (
            <Button onClick={handleSave}>Save order</Button>
          )}
        </ButtonGroup>
      </LoadingSpinner>
    </>
  );
};
export default ReorderIndicatorsList;
