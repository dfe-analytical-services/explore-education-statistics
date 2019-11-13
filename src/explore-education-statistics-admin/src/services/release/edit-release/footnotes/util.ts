import { FootnoteProps, FootnoteMeta, FootnoteMetaGetters } from './types';

const footnoteFormValidation = ({
  subjects,
  indicators,
  filters,
  filterGroups,
  filterItems,
}: FootnoteProps) => {
  const errors: { [key: string]: any } = {};
  const atLeastOneOption =
    [...subjects, ...indicators, ...filters, ...filterGroups, ...filterItems]
      .length === 0 &&
    'At least one Subject, Indicator or Filter must be selected';
  if (atLeastOneOption) {
    errors.subjects = atLeastOneOption;
  }
  return errors;
};

export const generateFootnoteMetaMap = (
  footnoteMeta: FootnoteMeta,
): FootnoteMetaGetters => {
  const filtersToSubject: { [key: number]: number } = {};
  const filterGroupsToFilters: { [key: number]: number } = {};
  const filterItemsToFilterGroups: { [key: number]: number } = {};
  const indicatorsToSubject: { [key: number]: number } = {};
  const indicatorItemsToIndicators: { [key: number]: number } = {};

  Object.keys(footnoteMeta).forEach(subjectId => {
    Object.keys(footnoteMeta[Number(subjectId)].indicators).forEach(
      indicatorId => {
        indicatorsToSubject[Number(indicatorId)] = Number(subjectId);

        Object.keys(
          footnoteMeta[Number(subjectId)].indicators[Number(indicatorId)]
            .options,
        ).forEach(indicatorItemId => {
          indicatorItemsToIndicators[Number(indicatorItemId)] = Number(
            indicatorId,
          );
        });
      },
    );

    Object.keys(footnoteMeta[Number(subjectId)].filters).forEach(filterId => {
      filtersToSubject[Number(filterId)] = Number(subjectId);

      Object.keys(
        footnoteMeta[Number(subjectId)].filters[Number(filterId)].options,
      ).forEach(filterGroupId => {
        filterGroupsToFilters[Number(filterGroupId)] = Number(filterId);

        Object.keys(
          footnoteMeta[Number(subjectId)].filters[Number(filterId)].options[
            Number(filterGroupId)
          ].options,
        ).forEach(filterItemId => {
          filterItemsToFilterGroups[Number(filterItemId)] = Number(
            filterGroupId,
          );
        });
      });
    });
  });

  const getItem = (
    ids: number[],
    itemType: 'subject' | 'indicator' | 'filter',
  ) => {
    //@ts-ignore
    if (ids.includes(undefined) || ids.length === 0) {
      return {
        label: 'A problem occurred',
        value: -1,
      };
    }

    const subject = footnoteMeta[ids[0]];
    if (itemType === 'subject') {
      return {
        label: subject.subjectName,
        value: ids[0],
      };
    }

    if (itemType === 'indicator') {
      const indicatorId = ids[1];
      const indicatorItemId = ids[2];

      const indicator = subject.indicators[indicatorId];
      if (indicatorItemId === undefined) {
        return { label: indicator.label, value: indicatorId };
      }
      const indicatorItem = indicator.options[indicatorItemId];
      return { label: indicatorItem.label, value: indicatorItemId };
    }

    if (itemType === 'filter') {
      const filterId = ids[1];
      const filterGroupId = ids[2];
      const filterItemId = ids[3];

      const filter = subject.filters[filterId];
      if (filterGroupId === undefined) {
        return { label: filter.legend, value: filterId };
      }
      const filterGroup = filter.options[filterGroupId];
      if (filterItemId === undefined) {
        return { label: filterGroup.label, value: filterGroupId };
      }
      const filterItem = filterGroup.options[filterItemId];
      return {
        label: filterItem.label,
        value: filterItemId,
      };
    }

    return {
      label: 'A problem occurred',
      value: -1,
    };
  };

  const getFilterItem = (filterItemId: number) => {
    const filterGroupId = filterItemsToFilterGroups[filterItemId];
    const filterId = filterGroupsToFilters[filterGroupId];
    const subjectId = filtersToSubject[filterId];

    return getItem(
      [subjectId, filterId, filterGroupId, filterItemId],
      'filter',
    );
  };
  const getFilterGroup = (filterGroupId: number) => {
    const filterId = filterGroupsToFilters[filterGroupId];
    const subjectId = filtersToSubject[filterId];

    return getItem([subjectId, filterId, filterGroupId], 'filter');
  };
  const getFilter = (filterId: number) => {
    const subjectId = filtersToSubject[filterId];

    return getItem([subjectId, filterId], 'filter');
  };
  const getIndicator = (indicatorItemId: number) => {
    const indicatorId = indicatorItemsToIndicators[indicatorItemId];
    const subjectId = indicatorsToSubject[indicatorId];

    return getItem([subjectId, indicatorId, indicatorItemId], 'indicator');
  };
  const getSubject = (subjectId: number) => {
    return getItem([subjectId], 'subject');
  };

  return {
    getFilterItem,
    getFilterGroup,
    getFilter,
    getIndicator,
    getSubject,
  };
};

export default footnoteFormValidation;
