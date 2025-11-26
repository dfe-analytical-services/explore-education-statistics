import { FilterHierarchyOption } from '@common/modules/table-tool/components/FilterHierarchyOptions';

export default function getFilterOptionIdsRecursively(
  optionTree: FilterHierarchyOption,
): string[] {
  const optionsIds =
    optionTree.options?.flatMap(getFilterOptionIdsRecursively) ?? [];
  return [optionTree.value, ...optionsIds];
}
