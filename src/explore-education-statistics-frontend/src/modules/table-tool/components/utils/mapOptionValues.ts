import {
  FilterOption,
  GroupedFilterOptions,
} from '@frontend/modules/table-tool/components/meta/initialSpec';

export default function mapOptionValues<T extends FilterOption>(
  options: GroupedFilterOptions | FilterOption[],
): { [key: string]: T } {
  if (Array.isArray(options)) {
    return options.reduce((acc, option) => {
      return {
        ...acc,
        [option.value]: {
          ...option,
        },
      };
    }, {});
  }

  return Object.values(options)
    .flatMap(group => group.options)
    .reduce((acc, option) => {
      return {
        ...acc,
        [option.value]: {
          ...option,
        },
      };
    }, {});
}
