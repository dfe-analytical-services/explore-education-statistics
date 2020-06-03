import { BaseFootnote } from '@admin/services/footnoteService';

interface FlatFootnote {
  content: string;
  subjects: string[];
  indicatorGroups: string[];
  indicators: string[];
  filters: string[];
  filterGroups: string[];
  filterItems: string[];
}

const footnoteToFlatFootnote = (footnote: BaseFootnote): FlatFootnote => {
  const subjects: string[] = [];
  const indicatorGroups: string[] = [];
  let indicators: string[] = [];
  const filters: string[] = [];
  const filterGroups: string[] = [];
  let filterItems: string[] = [];

  Object.entries(footnote.subjects).map(([subjectId, subject]) => {
    if (subject.selected) {
      return subjects.push(subjectId);
    }
    Object.entries(subject.indicatorGroups).map(
      ([indicatorGroupId, indicatorGroup]) => {
        if (indicatorGroup.selected) {
          return indicatorGroups.push(indicatorGroupId);
        }
        indicators = [...indicators, ...indicatorGroup.indicators];
        return null;
      },
    );
    return Object.entries(subject.filters).map(([filterId, filter]) => {
      if (filter.selected) {
        return filters.push(filterId);
      }
      return Object.entries(filter.filterGroups).map(
        ([filterGroupId, filterGroup]) => {
          if (filterGroup.selected) {
            return filterGroups.push(filterGroupId);
          }

          filterItems = [...filterItems, ...filterGroup.filterItems];
          return null;
        },
      );
    });
  });

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
export default footnoteToFlatFootnote;
