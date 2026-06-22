import { BaseFootnote, FootnoteMeta } from '@admin/services/footnoteService';

export interface FlatFootnote {
  content: string;
  subjects: string[];
  indicatorGroups: string[];
  indicators: string[];
  filters: string[];
  filterGroups: string[];
  filterItems: string[];
}

const footnoteToFlatFootnote = (
  footnote: BaseFootnote,
  footnoteMeta: FootnoteMeta,
): FlatFootnote => {
  const flatFootnote: FlatFootnote = {
    ...footnote,
    subjects: [],
    indicators: [],
    indicatorGroups: [],
    filters: [],
    filterGroups: [],
    filterItems: [],
  };

  Object.entries(footnote.subjects).forEach(([subjectId, subject]) => {
    if (subject.selectionType === 'All') {
      flatFootnote.subjects.push(subjectId);
    } else if (subject.selectionType !== 'NA') {
      Object.entries(subject.indicatorGroups).forEach(
        ([indicatorGroupId, indicatorGroup]) => {
          if (indicatorGroup.selected) {
            flatFootnote.indicatorGroups.push(indicatorGroupId);
          } else {
            flatFootnote.indicators.push(...indicatorGroup.indicators);
          }
        },
      );

      Object.entries(subject.filters).forEach(([filterId, filter]) => {
        if (filter.selected) {
          flatFootnote.filters.push(filterId);
        } else {
          Object.entries(filter.filterGroups).forEach(
            ([filterGroupId, filterGroup]) => {
              if (filterGroup.selected) {
                // filter group footnotes don't exist - so add every single filter item for that group
                flatFootnote.filterItems.push(
                  ...footnoteMeta.subjects[subjectId].filters[filterId].options[
                    filterGroupId
                  ].options.map(filterItem => filterItem.value),
                );
              } else {
                flatFootnote.filterItems.push(...filterGroup.filterItems);
              }
            },
          );
        }
      });
    }
  });

  return flatFootnote;
};
export default footnoteToFlatFootnote;
