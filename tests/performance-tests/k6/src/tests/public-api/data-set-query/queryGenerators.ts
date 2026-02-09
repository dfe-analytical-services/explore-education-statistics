/* eslint-disable max-classes-per-file */
import {
  DataSetMeta,
  DataSetQueryComparableOperator,
  DataSetQueryComparablePredicate,
  DataSetQueryCriteria,
  DataSetQueryIdOperator,
  DataSetQueryIdPredicate,
  DataSetQueryNode,
  DataSetQueryNodeType,
  DataSetQueryRequest,
  datSetQueryNodeTypes,
} from '../../../utils/publicApiService';
import { pickRandom, pickRandomItems } from '../../../utils/utils';

const geographicLevelQuerySupported = false;

class Range {
  lower: number;

  upper: number;

  constructor(lower: number, upper: number) {
    this.lower = lower;
    this.upper = upper;
  }

  random() {
    return this.lower + (this.upper - this.lower) * Math.random();
  }
}

class DataSetQueryGenerator {
  private readonly maxDepth: number;

  private readonly maxBranching: number;

  private readonly maxArrayItems: number;

  private readonly idOperators: DataSetQueryIdOperator[];

  private readonly comparableOperators: DataSetQueryComparableOperator[];

  constructor({
    maxDepth,
    maxBranching,
    maxArrayItems,
    idOperators,
    comparableOperators,
  }: QueryGeneratorConfig) {
    this.maxDepth = maxDepth;
    this.maxBranching = maxBranching ?? 3;
    this.maxArrayItems = maxArrayItems ?? Number.MAX_VALUE;
    this.idOperators = idOperators;
    this.comparableOperators = comparableOperators;
  }

  generateQuery(dataSetMeta: DataSetMeta): DataSetQueryRequest {
    const indicatorRange = new Range(0.1, 0.5);
    const indicators = pickRandomItems(
      dataSetMeta.indicators,
      Math.min(indicatorRange.random(), this.maxArrayItems),
    );
    return {
      criteria: this.generateDataSetQueryNode({
        dataSetMeta,
        currentDepth: 1,
      }),
      indicators: indicators.map(i => i.id),
    };
  }

  private generateDataSetQueryNode({
    dataSetMeta,
    currentDepth,
  }: {
    dataSetMeta: DataSetMeta;
    currentDepth: number;
  }): DataSetQueryNode {
    const allowedNodeTypes: DataSetQueryNodeType[] =
      currentDepth === this.maxDepth
        ? ['criteria', 'not']
        : [...datSetQueryNodeTypes];
    const nodeType = pickRandom(allowedNodeTypes);

    switch (nodeType) {
      case 'criteria':
      case 'not': {
        return this.generateDataSetCriteriaNode(dataSetMeta);
      }
      default: {
        const childNodeCount = Math.max(
          2,
          Math.ceil(this.maxBranching * Math.random()),
        );
        return {
          [nodeType]: Array.from({ length: childNodeCount }, _ =>
            this.generateDataSetQueryNode({
              dataSetMeta,
              currentDepth: currentDepth + 1,
            }),
          ),
        };
      }
    }
  }

  private generateIdListClause<T>(
    idList: T[],
    range: Range,
  ): DataSetQueryIdPredicate {
    const operator = pickRandom(this.idOperators);
    switch (operator) {
      case 'in':
      case 'notIn': {
        return {
          [operator]: pickRandomItems(
            idList,
            Math.min(range.random(), this.maxArrayItems),
          ),
        };
      }
      case 'eq':
      case 'notEq': {
        return {
          [operator]: pickRandom(idList),
        };
      }
      default: {
        throw new Error(`Unsupported id operator ${operator}`);
      }
    }
  }

  private generateComparableListClause<T>(
    comparableList: T[],
    range: Range,
  ): DataSetQueryComparablePredicate<T> {
    const operator = pickRandom(this.comparableOperators);
    switch (operator) {
      case 'in':
      case 'notIn': {
        return {
          [operator]: pickRandomItems(
            comparableList,
            Math.min(range.random(), this.maxArrayItems),
          ),
        };
      }
      case 'eq':
      case 'notEq': {
        return {
          [operator]: pickRandom(comparableList),
        };
      }
      case 'gt':
      case 'gte': {
        return {
          [operator]: pickRandom(comparableList.slice(-1)),
        };
      }
      case 'lt':
      case 'lte': {
        return {
          [operator]: pickRandom(comparableList.slice(1)),
        };
      }
      default:
        throw new Error(`Unsupported comparable operator ${operator}`);
    }
  }

  private generateDataSetCriteriaNode(
    dataSetMeta: DataSetMeta,
  ): DataSetQueryCriteria {
    const filterRange = new Range(0.1, 0.5);
    const filterItemRange = new Range(0.1, 0.5);
    const locationsRange = new Range(0.1, 0.5);
    const timePeriodsRange = new Range(0.1, 0.5);
    const geographicLevelRange = new Range(0.1, 0.5);

    const flatLocations = dataSetMeta.locations.flatMap(locationLevel =>
      locationLevel.options.map(locationOption => ({
        level: locationLevel.level.code,
        id: locationOption.id,
      })),
    );

    const flatFilterItems = pickRandomItems(
      dataSetMeta.filters,
      filterRange.random(),
    ).flatMap(f => f.options);

    const availableGeographicLevels = Object.keys(dataSetMeta.locations);

    const filterItems =
      flatFilterItems.length > 0
        ? this.generateIdListClause(
            flatFilterItems.map(fi => fi.id),
            filterItemRange,
          )
        : undefined;

    const locations = this.generateIdListClause(flatLocations, locationsRange);

    const geographicLevels = geographicLevelQuerySupported
      ? this.generateIdListClause(
          availableGeographicLevels,
          geographicLevelRange,
        )
      : undefined;

    const timePeriods = this.generateComparableListClause(
      dataSetMeta.timePeriods.map(tp => ({
        code: tp.code,
        period: tp.period,
      })),
      timePeriodsRange,
    );

    return {
      filters: filterItems,
      locations,
      timePeriods,
      geographicLevels,
    };
  }
}

export interface Dictionary<TValue> {
  [key: number]: TValue;
}

export function stringifySimplifiedQuery(query: DataSetQueryRequest) {
  // noinspection TypeScriptValidateJSTypes
  return JSON.stringify(
    query,
    (key, value) => {
      if (['indicators', 'in', 'notIn'].includes(key) && 'length' in value) {
        return `${value.length} options`;
      }
      return value;
    },
    2,
  );
}

export type QueryGeneratorConfig = {
  maxDepth: number;
  maxBranching?: number;
  maxArrayItems?: number;
  idOperators: DataSetQueryIdOperator[];
  comparableOperators: DataSetQueryComparableOperator[];
};

export default function createQueryGenerator(config: QueryGeneratorConfig) {
  return new DataSetQueryGenerator(config);
}
