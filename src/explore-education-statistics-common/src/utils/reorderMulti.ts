import { Filter } from '@common/modules/table-tool/types/filters';

interface Props {
  list: Filter[];
  sourceIndex: number;
  destinationIndex: number;
  selectedIds: string[];
}
const reorderMulti = ({
  list,
  sourceIndex,
  destinationIndex,
  selectedIds,
}: Props): Filter[] => {
  const destinationItemId = list[destinationIndex].value;
  const selectedList: Filter[] = list.filter(item =>
    selectedIds.includes(item.value),
  );
  const newList: Filter[] = list.filter(
    item => !selectedIds.includes(item.value),
  );

  let insertIndex = newList.findIndex(item => item.value === destinationItemId);

  if (sourceIndex < destinationIndex) {
    insertIndex += 1;
  }
  newList.splice(insertIndex, 0, ...selectedList);
  return newList;
};

export default reorderMulti;
