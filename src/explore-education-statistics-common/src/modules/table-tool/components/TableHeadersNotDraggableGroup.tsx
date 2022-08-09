import TableHeadersReadOnlyList from '@common/modules/table-tool/components/TableHeadersReadOnlyList';
import TableHeadersReorderableList from '@common/modules/table-tool/components/TableHeadersReorderableList';
import TableHeadersGroupControls from '@common/modules/table-tool/components/TableHeadersGroupControls';
import getTableHeaderGroupId from '@common/modules/table-tool/components/utils/getTableHeaderGroupId';
import useTableHeadersContext from '@common/modules/table-tool/contexts/TableHeadersContext';
import styles from '@common/modules/table-tool/components/TableHeadersDraggableGroup.module.scss';
import React from 'react';

interface Props {
  legend: string;
  name: string;
  totalItems: number;
}

const TableHeadersNotDraggableGroup = ({ legend, name, totalItems }: Props) => {
  const { activeList } = useTableHeadersContext();
  const groupId = getTableHeaderGroupId(legend);
  const isActive = activeList === groupId;

  return (
    <>
      <div className={styles.group}>
        {isActive ? (
          <TableHeadersReorderableList
            id={groupId}
            legend={legend}
            name={name}
          />
        ) : (
          <TableHeadersReadOnlyList
            id={groupId}
            isDraggable={false}
            legend={legend}
            name={name}
          />
        )}
      </div>

      <TableHeadersGroupControls
        axisName={name as string}
        id={groupId}
        legend={legend}
        totalItems={totalItems}
      />
    </>
  );
};

export default TableHeadersNotDraggableGroup;
