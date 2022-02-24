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
  SubjectMeta,
} from '@common/services/tableBuilderService';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';

// EES-1243 - remove this when the backend is done
const testSubjectMeta: SubjectMeta = {
  filters: {
    Category1: {
      id: 'category-1-id',
      legend: 'Category 1',
      name: 'category-1',
      options: {
        Category1Group1: {
          id: 'category-1-group-1-id',
          label: 'Category 1 Group 1',
          options: [
            {
              label: 'Category 1 Group 1 Item 1',
              value: 'category-1-group-1-item-1',
              id: 'category-1-group-1-item-1-id',
            },
          ],
          order: 1,
        },
        Category1Group2: {
          id: 'category-1-group-2-id',
          label: 'Category 1 Group 2',
          options: [
            {
              label: 'Category 1 Group 2 Item 1',
              value: 'category-1-group-2-item-1',
              id: 'category-1-group-2-item-1-id',
            },
            {
              label: 'Category 1 Group 2 Item 2',
              value: 'category-1-group-2-item-2',
              id: 'category-1-group-2-item-2-id',
            },
            {
              label: 'Category 1 Group 2 Item 3',
              value: 'category-1-group-2-item-3',
              id: 'category-1-group-2-item-3-id',
            },
          ],
          order: 0,
        },
        Category1Group3: {
          id: 'category-1-group-3-id',
          label: 'Category 1 Group 3',
          options: [
            {
              label: 'Category 1 Group 3 Item 1',
              value: 'category-1-group-3-item-1',
              id: 'category-1-group-3-item-1-id',
            },
            {
              label: 'Category 1 Group 3 Item 2',
              value: 'category-1-group-3-item-2',
              id: 'category-1-group-3-item-2-id',
            },
          ],
          order: 2,
        },
      },
      order: 1,
    },
    Category2: {
      id: 'category-2-id',
      legend: 'Category 2',
      name: 'category-2',
      options: {
        Category2Group1: {
          id: 'category-2-group-1-id',
          label: 'Category 2 Group 1',
          options: [
            {
              label: 'Category 2 Group 1 Item 1',
              value: 'category-2-group-1-item-1',
              id: 'category-2-group-1-item-1-id',
            },
            {
              label: 'Category 2 Group 1 Item 2',
              value: 'category-2-group-1-item-2',
              id: 'category-2-group-1-item-2-id',
            },
          ],
          order: 0,
        },
      },
      order: 0,
    },
  },
  indicators: {},
  locations: {},
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
};

interface UpdatedFilter {
  id?: string;
  filterGroups: { id?: string; filterItems: (string | undefined)[] }[]; // EES-1243 - won't need the undefined when ids exist
}
export type UpdateFiltersRequest = UpdatedFilter[];

interface Props {
  subject: Subject;
  onCancel: () => void;
  onSave: (requestFilters: UpdateFiltersRequest) => void;
}

const ReorderFiltersList = ({ subject, onCancel, onSave }: Props) => {
  const { value: subjectMeta, isLoading } = useAsyncHandledRetry(
    () => tableBuilderService.getSubjectMeta(subject.id),
    [subject.id],
  );

  const [filters, setFilters] = useState<FormattedFilters[]>([]);

  useEffect(() => {
    if (isLoading) {
      return;
    }
    // EES-1243 - restore this line to use the real filters.
    // const filtersMeta = subjectMeta?.filters || {};
    const filtersMeta = testSubjectMeta.filters;

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
                items: item.options,
              };
            }),
            'order',
          ),
        };
      }),
      'order',
    );
    setFilters(formattedFilters);
  }, [isLoading, subjectMeta?.filters]);

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
      if (!parentGroupId || parentGroupId === parentCategoryId) {
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
    onSave(updateFiltersRequest);
  };

  return (
    <>
      <h3>Reorder filters for {subject.name}</h3>
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
