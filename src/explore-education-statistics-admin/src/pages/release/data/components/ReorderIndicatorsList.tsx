import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ReorderableNestedList from '@common/components/ReorderableNestedList';
import {
  ReorderableListItem,
  ReorderResult,
} from '@common/components/ReorderableItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import reorder from '@common/utils/reorder';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';

interface ReorderHandler extends ReorderResult {
  expandedItemId?: string;
}

interface UpdatedIndicator {
  id: string;
  indicators: string[];
}
export type UpdateIndicatorsRequest = UpdatedIndicator[];

interface Props {
  releaseVersionId: string;
  subject: Subject;
  onCancel: () => void;
  onSave: (
    subjectId: string,
    orderedIndicators: UpdateIndicatorsRequest,
  ) => void;
}

export default function ReorderIndicatorsList({
  releaseVersionId,
  subject,
  onCancel,
  onSave,
}: Props) {
  const { value: subjectMeta, isLoading } = useAsyncHandledRetry(
    () => tableBuilderService.getSubjectMeta(subject.id, releaseVersionId),
    [subject.id, releaseVersionId],
  );

  const [indicators, setIndicators] = useState<ReorderableListItem[]>([]);

  useEffect(() => {
    if (isLoading || !subjectMeta) {
      return;
    }
    const indicatorsMeta = subjectMeta?.indicators || {};

    // Transforming the indicators to be nested arrays rather than keyed objects.
    // Order by the order field.
    const formattedIndicators: ReorderableListItem[] = orderBy(
      Object.values(indicatorsMeta),
      'order',
    ).map(item => {
      return {
        id: item.id,
        label: item.label,
        childOptions: item.options.map(option => ({
          id: option.value,
          label: option.label,
        })),
      };
    });

    setIndicators(formattedIndicators);
  }, [isLoading, subjectMeta]);

  const handleReorder = ({
    prevIndex,
    nextIndex,
    expandedItemId,
  }: ReorderHandler) => {
    // Top level
    if (!expandedItemId) {
      setIndicators(reorder(indicators, prevIndex, nextIndex));
      return;
    }
    // Second level
    const reordered = indicators.map(indicator =>
      indicator.id !== expandedItemId
        ? indicator
        : {
            ...indicator,
            childOptions: indicator.childOptions
              ? reorder(indicator.childOptions, prevIndex, nextIndex)
              : [],
          },
    );

    setIndicators(reordered);
  };

  const handleSave = () => {
    const updateIndicatorsRequest: UpdateIndicatorsRequest = indicators.map(
      indicator => {
        return {
          id: indicator.id,
          indicators: indicator.childOptions
            ? indicator.childOptions?.map(item => item.id)
            : [],
        };
      },
    );
    onSave(subject.id, updateIndicatorsRequest);
  };

  return (
    <LoadingSpinner loading={isLoading}>
      {indicators.length === 0 ? (
        <>
          <p>No indicators available.</p>
          <Button variant="secondary" onClick={onCancel}>
            Back
          </Button>
        </>
      ) : (
        <>
          <ReorderableNestedList
            heading={`Reorder indicators for ${subject.name}`}
            id="reorder-indicators"
            list={indicators}
            testId="reorder-indicators"
            onMoveItem={handleReorder}
          />
          <ButtonGroup>
            {indicators.length > 0 && (
              <Button onClick={handleSave}>Save order</Button>
            )}
            <Button variant="secondary" onClick={onCancel}>
              Cancel
            </Button>
          </ButtonGroup>
        </>
      )}
    </LoadingSpinner>
  );
}
