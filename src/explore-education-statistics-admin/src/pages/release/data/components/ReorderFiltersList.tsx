import ReorderList, {
  FormattedGroup,
  FormattedFilters,
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

interface UpdatedFilter {
  id: string;
  filterGroups: { id: string; filterItems: string[] }[];
}
export type UpdateFiltersRequest = UpdatedFilter[];

interface Props {
  subject: Subject;
  releaseId: string;
  onCancel: () => void;
  onSave: (subjectId: string, requestFilters: UpdateFiltersRequest) => void;
}

const ReorderFiltersList = ({
  releaseId,
  subject,
  onCancel,
  onSave,
}: Props) => {
  const { value: subjectMeta, isLoading } = useAsyncHandledRetry(
    () => tableBuilderService.getSubjectMeta(subject.id, releaseId),
    [subject.id, releaseId],
  );

  const [filters, setFilters] = useState<FormattedFilters[]>([]);

  useEffect(() => {
    if (isLoading || !subjectMeta) {
      return;
    }
    const filtersMeta = subjectMeta?.filters || {};

    // Transforming the filters to be nested arrays rather than keyed objects.
    // Order by the order fields.
    const formattedFilters = orderBy(
      Object.entries(filtersMeta).map(([, group]) => {
        return {
          id: group.id,
          label: group.legend,
          order: group.order,
          groups: orderBy(
            Object.entries(group.options).map(([, item]) => {
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
          ),
        };
      }),
      'order',
    );
    setFilters(formattedFilters);
  }, [isLoading, subjectMeta]);

  const handleReorder = ({
    reordered,
    parentCategoryId,
    parentGroupId,
  }: ReorderProps) => {
    // reordering filters
    if (!parentCategoryId && !parentGroupId) {
      setFilters(reordered as FormattedFilters[]);
      return;
    }

    const reorderedFilters = filters.map(filter => {
      if (filter.id !== parentCategoryId) {
        return filter;
      }
      // Reordering groups
      if (!parentGroupId) {
        return { ...filter, groups: reordered };
      }
      // Reordering items
      const updatedFilterGroups = filter.groups.map(
        (filterGroup: FormattedGroup) => {
          if (filterGroup.id !== parentGroupId) {
            return filterGroup;
          }
          return {
            ...filterGroup,
            items: reordered,
          };
        },
      );

      return { ...filter, groups: updatedFilterGroups };
    }) as FormattedFilters[];

    setFilters(reorderedFilters);
  };

  const handleSave = () => {
    const updateFiltersRequest: UpdateFiltersRequest = filters.map(filter => {
      return {
        id: filter.id,
        filterGroups: filter.groups.map(group => {
          return {
            id: group.id,
            filterItems: group.items.map(item => item.id),
          };
        }),
      };
    });
    onSave(subject.id, updateFiltersRequest);
  };

  return (
    <>
      <h3>{`Reorder filters for ${subject.name}`}</h3>
      <LoadingSpinner loading={isLoading}>
        {filters.length === 0 ? (
          <p>No filters available.</p>
        ) : (
          <ReorderList listItems={filters} onReorder={handleReorder} />
        )}
        <ButtonGroup>
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
          {filters.length > 0 && (
            <Button onClick={handleSave}>Save order</Button>
          )}
        </ButtonGroup>
      </LoadingSpinner>
    </>
  );
};
export default ReorderFiltersList;
