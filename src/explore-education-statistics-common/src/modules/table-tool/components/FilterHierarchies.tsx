import type {
  FilterHierarchy as FilterHierarchyType,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import React, { useCallback, useMemo } from 'react';
import { Dictionary } from '@common/types';
import sortBy from 'lodash/sortBy';

interface Props {
  filterHierachy: FilterHierarchyType;
  filters: SubjectMeta['filters'];
}

function getColumnClass(numerator: number, denominator: number): string {
  if (
    numerator % 1 !== 0 ||
    numerator < 0 ||
    numerator > 3 ||
    denominator % 1 !== 0 ||
    denominator < 2 ||
    denominator > 4 ||
    numerator === denominator
  ) {
    return '';
  }

  const prefix = 'govuk-grid-column';
  const numbers = ['one', 'two', 'three'];
  const chunkSizes = ['half', 'third', 'quarter'];

  return [
    prefix,
    numbers[numerator - 1],
    `${chunkSizes[denominator - 2]}${numerator > 1 ? 's' : ''}`,
  ].join('-');
}

type FilterHierarchyOptionsTree = {
  id: string;
  label: string;
  options?: FilterHierarchyOptionsTree;
}[];

export default function FilterHierarchy({
  filters,
  filterHierachy: filterHierarchy,
}: Props) {
  const totalColumns = filterHierarchy.childFilterIds.length + 1;
  const [labels, autoSelectIds] = useMemo(() => {
    const labelsMap: Dictionary<string> = {};
    const autoSelectFilterItemIds: string[] = [];

    Object.values(filters).forEach(filter => {
      if (filter.autoSelectFilterItemId) {
        autoSelectFilterItemIds.push(filter.autoSelectFilterItemId);
      }
      labelsMap[filter.id] = filter.legend;
      Object.values(filter.options).forEach(filterGroups => {
        labelsMap[filterGroups.id] = filterGroups.label;
        filterGroups.options.forEach(filterGroupOption => {
          labelsMap[filterGroupOption.value] = filterGroupOption.label;
        });
      });
    });

    return [labelsMap, autoSelectFilterItemIds];
  }, [filters]);

  const sortOptions = useCallback(
    (options: string[]) => [
      ...options.filter(a => {
        return (
          autoSelectIds.indexOf(a) !== -1 ||
          labels[a].toLocaleLowerCase() === 'total'
        );
      }),
      ...options.filter(a => {
        return (
          autoSelectIds.indexOf(a) === -1 &&
          labels[a].toLocaleLowerCase() !== 'total'
        );
      }),
    ],
    [autoSelectIds],
  );

  const filterHierarchyOptions = useMemo<FilterHierarchyOptionsTree>(() => {
    const tiers = sortBy(filterHierarchy.tiers, 'level');

    function getTierOptions({
      currentLevel = 0,
      currentOptionId,
    }: {
      currentOptionId: string;
      currentLevel?: number;
    }): FilterHierarchyOptionsTree | undefined {
      if (currentLevel === totalColumns - 1) {
        return undefined;
      }

      const tierOptions = tiers[currentLevel]?.hierarchy[currentOptionId] ?? [];

      return sortOptions(tierOptions).map(childOptionId => ({
        id: childOptionId,
        label: labels[childOptionId],
        options: getTierOptions({
          currentLevel: currentLevel + 1,
          currentOptionId: childOptionId,
        }),
      }));
    }

    return sortOptions(filterHierarchy.rootOptionIds).map(
      firstLevelOptionId => {
        return {
          id: firstLevelOptionId,
          label: labels[firstLevelOptionId],
          options: getTierOptions({ currentOptionId: firstLevelOptionId }),
        };
      },
    );
  }, [filterHierarchy, labels, totalColumns]);

  function renderOptions(
    optionsTree?: FilterHierarchyOptionsTree,
    level: number = 0,
  ) {
    if (!optionsTree) {
      return undefined;
    }

    return optionsTree.map(({ id, label, options }) => (
      <div key={id} className="govuk-grid-row govuk-!-margin-bottom-2">
        <hr />
        <div className={getColumnClass(1, totalColumns - level)}>{label}</div>
        <div
          className={getColumnClass(
            totalColumns - level - 1,
            totalColumns - level,
          )}
        >
          {renderOptions(options, level + 1)}
        </div>
      </div>
    ));
  }

  return (
    <div>
      <div>
        <div
          datatest-id="filter-hierarchy-header-row"
          className="govuk-grid-row"
        >
          <div className={getColumnClass(1, totalColumns)}>
            <h4>{labels[filterHierarchy.rootFilterId]}</h4>
          </div>

          {filterHierarchy.childFilterIds.map(filterId => (
            <div className={getColumnClass(1, totalColumns)}>
              <h4>{labels[filterId]}</h4>
            </div>
          ))}
        </div>
      </div>

      <div datatest-id="filter-hierarchy-first-options-tree">
        {renderOptions(filterHierarchyOptions)}
      </div>
    </div>
  );
}
