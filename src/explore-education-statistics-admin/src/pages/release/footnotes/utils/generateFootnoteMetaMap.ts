import { FootnoteMeta } from '@admin/services/footnoteService';

export interface FootnoteMetaGetters {
  getSubject: (id: string) => { label: string; value: string };
  getIndicatorGroup: (id: string) => { label: string; value: string };
  getIndicator: (id: string) => { label: string; value: string };
  getFilter: (id: string) => { label: string; value: string };
  getFilterGroup: (id: string) => { label: string; value: string };
  getFilterItem: (id: string) => { label: string; value: string };
}

const generateFootnoteMetaMap = (
  footnoteMeta: FootnoteMeta,
): FootnoteMetaGetters => {
  const filtersToSubject: { [key: string]: string } = {};
  const filterGroupsToFilters: { [key: string]: string } = {};
  const filterItemsToFilterGroups: { [key: string]: string } = {};
  const indicatorsToSubject: { [key: string]: string } = {};
  const indicatorItemsToIndicators: { [key: string]: string } = {};

  Object.entries(footnoteMeta.subjects).forEach(([subjectId, subject]) => {
    Object.entries(subject.indicators).forEach(
      ([indicatorGroupId, indicatorGroup]) => {
        indicatorsToSubject[indicatorGroupId] = subjectId;

        indicatorGroup.options.forEach(indicator => {
          indicatorItemsToIndicators[indicator.value] = indicatorGroupId;
        });
      },
    );

    Object.entries(subject.filters).forEach(([filterId, filter]) => {
      filtersToSubject[filterId] = subjectId;

      Object.entries(filter.options).forEach(([filterGroupId, filterGroup]) => {
        filterGroupsToFilters[filterGroupId] = filterId;

        filterGroup.options.forEach(filterItem => {
          filterItemsToFilterGroups[filterItem.value] = filterGroupId;
        });
      });
    });
  });

  const getItem = (
    ids: string[],
    itemType: 'subject' | 'indicator' | 'filter',
  ) => {
    if (
      (ids as (string | undefined)[]).includes(undefined) ||
      ids.length === 0
    ) {
      return {
        label: 'A problem occurred',
        value: '-1',
      };
    }

    const subject = footnoteMeta.subjects[ids[0]];
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
      const indicatorItem = indicator.options.find(
        option => option.value === indicatorItemId,
      );
      return { label: indicatorItem?.label ?? '', value: indicatorItemId };
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
      const filterItem = filterGroup.options.find(
        option => option.value === filterItemId,
      );
      return {
        label: filterItem?.label ?? '',
        value: filterItemId,
      };
    }

    return {
      label: 'A problem occurred',
      value: '-1',
    };
  };

  const getSubject = (subjectId: string) => {
    return getItem([subjectId], 'subject');
  };
  const getIndicatorGroup = (indicatorId: string) => {
    const subjectId = indicatorsToSubject[indicatorId];

    return getItem([subjectId, indicatorId], 'indicator');
  };
  const getIndicator = (indicatorItemId: string) => {
    const indicatorId = indicatorItemsToIndicators[indicatorItemId];
    const subjectId = indicatorsToSubject[indicatorId];

    return getItem([subjectId, indicatorId, indicatorItemId], 'indicator');
  };
  const getFilter = (filterId: string) => {
    const subjectId = filtersToSubject[filterId];

    return getItem([subjectId, filterId], 'filter');
  };
  const getFilterGroup = (filterGroupId: string) => {
    const filterId = filterGroupsToFilters[filterGroupId];
    const subjectId = filtersToSubject[filterId];

    return getItem([subjectId, filterId, filterGroupId], 'filter');
  };
  const getFilterItem = (filterItemId: string) => {
    const filterGroupId = filterItemsToFilterGroups[filterItemId];
    const filterId = filterGroupsToFilters[filterGroupId];
    const subjectId = filtersToSubject[filterId];

    return getItem(
      [subjectId, filterId, filterGroupId, filterItemId],
      'filter',
    );
  };

  return {
    getSubject,
    getIndicatorGroup,
    getIndicator,
    getFilter,
    getFilterGroup,
    getFilterItem,
  };
};

export default generateFootnoteMetaMap;
