import DragIcon from '@common/components/DragIcon';
import styles from '@common/modules/table-tool/components/TableHeadersReadOnlyList.module.scss';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import { Filter } from '@common/modules/table-tool/types/filters';
import React from 'react';
import { useFormContext } from 'react-hook-form';

interface Props {
  defaultNumberOfItems: number;
  id: string;
  isDraggable?: boolean;
  legend: string;
  name: string;
}

const TableHeadersReadOnlyList = ({
  defaultNumberOfItems,
  id,
  isDraggable = true,
  legend,
  name,
}: Props) => {
  const { expandedLists, reverseOrderIds, isReverse, setIsReverse } =
    useTableHeadersContext();
  const isExpanded = expandedLists.includes(id);

  const { getValues } = useFormContext();
  const list: Filter[] = getValues(name);

  const displayItems = isExpanded ? list.length : defaultNumberOfItems;

  if (reverseOrderIds.includes(name) && isReverse) {
    list.reverse();
    setIsReverse(false);
  }

  return (
    <>
      <h4 className={styles.heading}>
        {legend} {isDraggable && <DragIcon className={styles.dragIcon} />}
      </h4>
      <ol
        className={styles.list}
        id={id}
        // eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex
        tabIndex={isExpanded ? 0 : undefined}
      >
        {list.slice(0, displayItems).map(option => (
          <li key={option.value} className={styles.item}>
            {option.label}
          </li>
        ))}
      </ol>
    </>
  );
};

export default TableHeadersReadOnlyList;
