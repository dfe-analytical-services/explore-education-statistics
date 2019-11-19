import {
  FootnoteProps,
  FootnoteMeta,
  FootnoteMetaGetters,
  FlatFootnoteProps,
} from './types';

export const footnoteToFlatFootnote = (
  footnote: FootnoteProps,
): FlatFootnoteProps => {
  const subjects = [] as string[];
  const indicatorGroups = [] as string[];
  const filters = [] as string[];
  const indicators = [] as string[];
  const filterGroups = [] as string[];
  const filterItems = [] as string[];

  return {
    ...footnote,
    subjects,
    indicatorGroups,
    indicators,
    filters,
    filterGroups,
    filterItems,
  };
};

const footnoteFormValidation = (footnote: FootnoteProps) => {
  const errors: { [key: string]: string } = {};
  if (footnote) {
    const atLeastOneOption =
      // [...subjects, ...indicators, ...filters, ...filterGroups, ...filterItems]
      //   .length === 0 &&
      // 'At least one Subject, Indicator or Filter must be selected';
      '';
    if (atLeastOneOption) {
      errors.subjects = atLeastOneOption;
    }
  }
  return errors;
};

export const generateFootnoteMetaMap = (
  footnoteMeta: FootnoteMeta,
): FootnoteMetaGetters => {
  const filtersToSubject: { [key: string]: string } = {};
  const filterGroupsToFilters: { [key: string]: string } = {};
  const filterItemsToFilterGroups: { [key: string]: string } = {};
  const indicatorsToSubject: { [key: string]: string } = {};
  const indicatorItemsToIndicators: { [key: string]: string } = {};

  Object.keys(footnoteMeta).forEach(subjectId => {
    Object.keys(footnoteMeta[subjectId].indicators).forEach(indicatorId => {
      indicatorsToSubject[indicatorId] = subjectId;

      Object.keys(
        footnoteMeta[subjectId].indicators[indicatorId].options,
      ).forEach(indicatorItemId => {
        indicatorItemsToIndicators[indicatorItemId] = indicatorId;
      });
    });

    Object.keys(footnoteMeta[subjectId].filters).forEach(filterId => {
      filtersToSubject[filterId] = subjectId;

      Object.keys(footnoteMeta[subjectId].filters[filterId].options).forEach(
        filterGroupId => {
          filterGroupsToFilters[filterGroupId] = filterId;

          Object.keys(
            footnoteMeta[subjectId].filters[filterId].options[filterGroupId]
              .options,
          ).forEach(filterItemId => {
            filterItemsToFilterGroups[filterItemId] = filterGroupId;
          });
        },
      );
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

export default footnoteFormValidation;
