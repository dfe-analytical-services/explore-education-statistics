import { SubjectMeta, TableQuery } from './types';

export function createTableBuilderQuery({
  subjectId,
  subjectMeta,
}: {
  subjectId: string;
  subjectMeta: SubjectMeta;
}): TableQuery {
  const oneFilterItemIdFromEachFilter = Object.values(
    subjectMeta.filters,
  ).flatMap(filter =>
    Object.values(filter.options)
      .flatMap(filterGroup =>
        filterGroup.options.flatMap(filterItem => filterItem.value),
      )
      .slice(0, 1),
  );

  const allOtherFilterItemIds = Object.values(subjectMeta.filters).flatMap(
    filter =>
      Object.values(filter.options)
        .flatMap(filterGroup =>
          filterGroup.options.flatMap(filterItem => filterItem.value),
        )
        .slice(1),
  );

  const maxSelectedFilterItemIds = 10;

  const someFilterItemIds = [
    ...oneFilterItemIdFromEachFilter,
    ...allOtherFilterItemIds.slice(
      0,
      Math.min(
        allOtherFilterItemIds.length,
        maxSelectedFilterItemIds - oneFilterItemIdFromEachFilter.length,
      ),
    ),
  ];

  const allIndicationIds = Object.values(
    subjectMeta.indicators,
  ).flatMap(indicatorGroup =>
    indicatorGroup.options.map(indicator => indicator.value),
  );

  const allLocationIds = Object.values(subjectMeta.locations).flatMap(
    geographicLevel =>
      geographicLevel.options.flatMap(location => {
        if (location.options) {
          return location.options.flatMap(o => o.id);
        }
        return [location.id];
      }),
  );

  const someLocationIds = allLocationIds.slice(
    0,
    Math.min(allLocationIds.length, 20),
  );

  const someTimePeriods = {
    startYear: subjectMeta.timePeriod.options[0].year,
    startCode: subjectMeta.timePeriod.options[0].code,
    endYear:
      subjectMeta.timePeriod.options[
        Math.max(2, subjectMeta.timePeriod.options.length) - 1
      ].year,
    endCode:
      subjectMeta.timePeriod.options[
        Math.max(2, subjectMeta.timePeriod.options.length) - 1
      ].code,
  };

  return {
    subjectId,
    filters: someFilterItemIds,
    indicators: allIndicationIds,
    locationIds: someLocationIds as string[],
    timePeriod: someTimePeriods,
  };
}

export default {};
