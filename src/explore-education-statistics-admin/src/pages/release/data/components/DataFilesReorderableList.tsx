import { DataFile } from '@admin/services/releaseDataFileService';
import ReorderableList from '@common/components/ReorderableList';
import reorder from '@common/utils/reorder';
import React, { useEffect, useMemo, useState } from 'react';

interface Props {
  dataFiles: DataFile[];
  onCancelReordering: () => void;
  onConfirmReordering: (dataFiles: DataFile[]) => void;
}

export default function DataFilesReorderableList({
  dataFiles: initialDataFiles,
  onCancelReordering,
  onConfirmReordering,
}: Props) {
  const [dataFiles, setDataFiles] = useState<DataFile[]>(initialDataFiles);

  useEffect(() => {
    setDataFiles(initialDataFiles);
  }, [initialDataFiles]);

  const list = useMemo(
    () =>
      dataFiles.map(({ id, title }) => ({
        id,
        label: title,
      })),
    [dataFiles],
  );

  return (
    <ReorderableList
      heading="Reorder data files"
      id="dataFiles"
      list={list}
      onCancel={() => {
        setDataFiles(initialDataFiles);
        onCancelReordering();
      }}
      onConfirm={() => {
        onConfirmReordering(dataFiles);
      }}
      onMoveItem={({ prevIndex, nextIndex }) => {
        setDataFiles(reorder(dataFiles, prevIndex, nextIndex));
      }}
      onReverse={() => {
        setDataFiles(dataFiles.toReversed());
      }}
    />
  );
}
