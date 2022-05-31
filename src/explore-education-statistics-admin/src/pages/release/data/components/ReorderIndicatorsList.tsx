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
} from '@common/services/tableBuilderService';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';

interface UpdatedIndicator {
  id: string;
  indicators: string[];
}
export type UpdateIndicatorsRequest = UpdatedIndicator[];

interface Props {
  releaseId: string;
  subject: Subject;
  onCancel: () => void;
  onSave: (
    subjectId: string,
    orderedIndicators: UpdateIndicatorsRequest,
  ) => void;
}

const ReorderIndicatorsList = ({
  releaseId,
  subject,
  onCancel,
  onSave,
}: Props) => {
  const { value: subjectMeta, isLoading } = useAsyncHandledRetry(
    () => tableBuilderService.getSubjectMeta(subject.id, releaseId),
    [subject.id, releaseId],
  );

  const [indicators, setIndicators] = useState<FormattedIndicators[]>([]);

  useEffect(() => {
    if (isLoading || !subjectMeta) {
      return;
    }
    const indicatorsMeta = subjectMeta?.indicators || {};

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
  }, [isLoading, subjectMeta]);

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
          indicators: indicator.items.map(item => item.id),
        };
      },
    );
    onSave(subject.id, updateIndicatorsRequest);
  };

  return (
    <>
      <h3>{`Reorder indicators for ${subject.name}`}</h3>
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
