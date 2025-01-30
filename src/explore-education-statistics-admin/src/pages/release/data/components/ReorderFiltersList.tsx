import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import {
  ReorderableListItem,
  ReorderResult,
} from '@common/components/ReorderableItem';
import ReorderableNestedList from '@common/components/ReorderableNestedList';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import reorder from '@common/utils/reorder';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';

interface ReorderHandler extends ReorderResult {
  expandedItemId?: string;
  expandedItemParentId?: string;
}

interface UpdatedFilter {
  id: string;
  filterGroups: { id: string; filterItems: string[] }[];
}
export type UpdateFiltersRequest = UpdatedFilter[];

interface Props {
  subject: Subject;
  releaseVersionId: string;
  onCancel: () => void;
  onSave: (subjectId: string, requestFilters: UpdateFiltersRequest) => void;
}

export default function ReorderFiltersList({
  releaseVersionId,
  subject,
  onCancel,
  onSave,
}: Props) {
  const { value: subjectMeta, isLoading } = useAsyncHandledRetry(
    () => tableBuilderService.getSubjectMeta(subject.id, releaseVersionId),
    [subject.id, releaseVersionId],
  );

  const [filters, setFilters] = useState<ReorderableListItem[]>([]);

  useEffect(() => {
    if (isLoading || !subjectMeta) {
      return;
    }
    const filtersMeta = subjectMeta?.filters || {};

    // Transforming the filters to be nested arrays rather than keyed objects.
    // Order by the order fields.
    const formattedFilters: ReorderableListItem[] = orderBy(
      Object.values(filtersMeta),
      'order',
    ).map(group => {
      return {
        id: group.id,
        label: group.legend,
        childOptions: orderBy(Object.values(group.options), 'order').map(
          item => ({
            id: item.id,
            label: item.label,
            parentId: group.id,
            childOptions: item.options.map(option => ({
              id: option.value,
              label: option.label,
              parentId: item.id,
            })),
          }),
        ),
      };
    });

    setFilters(formattedFilters);
  }, [isLoading, subjectMeta]);

  const handleReorder = ({
    prevIndex,
    nextIndex,
    expandedItemId,
    expandedItemParentId,
  }: ReorderHandler) => {
    // Top level
    if (!expandedItemId) {
      setFilters(reorder(filters, prevIndex, nextIndex));
      return;
    }
    const reordered = filters.map(filter => {
      // Second level
      if (filter.id === expandedItemId && filter.childOptions?.length) {
        // Only one child option when has default group.
        // In this case the children of the group are shown instead of the group.
        return filter.childOptions.length === 1
          ? {
              ...filter,
              childOptions: [
                {
                  ...filter.childOptions[0],
                  childOptions: filter.childOptions[0].childOptions
                    ? reorder(
                        filter.childOptions[0].childOptions,
                        prevIndex,
                        nextIndex,
                      )
                    : [],
                },
              ],
            }
          : {
              ...filter,
              childOptions: reorder(filter.childOptions, prevIndex, nextIndex),
            };
      }

      // Third level
      if (expandedItemParentId) {
        return filter.id === expandedItemParentId
          ? {
              ...filter,
              childOptions: filter.childOptions?.map(option =>
                option.id === expandedItemId
                  ? {
                      ...option,
                      childOptions: option.childOptions?.length
                        ? reorder(option.childOptions, prevIndex, nextIndex)
                        : [],
                    }
                  : option,
              ),
            }
          : filter;
      }
      return filter;
    });

    setFilters(reordered);
  };

  const handleSave = () => {
    const updateFiltersRequest: UpdateFiltersRequest = filters.map(filter => ({
      id: filter.id,
      filterGroups: filter.childOptions
        ? filter.childOptions?.map(group => ({
            id: group.id,
            filterItems: group.childOptions
              ? group.childOptions?.map(item => item.id)
              : [],
          }))
        : [],
    }));
    onSave(subject.id, updateFiltersRequest);
  };

  return (
    <LoadingSpinner loading={isLoading}>
      {filters.length === 0 ? (
        <>
          <p>No filters available.</p>
          <Button variant="secondary" onClick={onCancel}>
            Back
          </Button>
        </>
      ) : (
        <>
          <ReorderableNestedList
            heading={`Reorder filters for ${subject.name}`}
            id="reorder-filters"
            list={filters}
            testId="reorder-filters"
            onMoveItem={handleReorder}
          />
          <ButtonGroup>
            {filters.length > 0 && (
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
