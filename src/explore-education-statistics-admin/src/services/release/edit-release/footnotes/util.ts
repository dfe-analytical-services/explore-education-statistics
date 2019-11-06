import { FootnoteProps, FootnoteMeta, FootnoteMetaMap } from './types';

const footnoteFormValidation = (values: FootnoteProps) => {
  return 'atleast one filter must be selected';
};

export const generateFootnoteMetaMap = (
  footnoteMeta: FootnoteMeta,
): FootnoteMetaMap => {
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
          footnoteMeta[Number(subjectId)].indicators[Number(indicatorId)],
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

  const getFilterItem = (id: number) => {};

  return {
    filterItemsToFilterGroups,
    filterGroupsToFilters,
    filtersToSubject,
    indicatorsToSubject,
  };
};

export default footnoteFormValidation;
