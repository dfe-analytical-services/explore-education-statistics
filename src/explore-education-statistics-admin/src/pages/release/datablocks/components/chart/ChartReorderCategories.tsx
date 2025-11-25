import ButtonText from '@common/components/ButtonText';
import ModalConfirm from '@common/components/ModalConfirm';
import ReorderableList from '@common/components/ReorderableList';
import VisuallyHidden from '@common/components/VisuallyHidden';
import {
  MapCategoricalData,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import reorder from '@common/utils/reorder';
import React, { useState } from 'react';

interface Props {
  label: string;
  mapDataSetConfig: MapDataSetConfig;
  onReorder: (updated: MapDataSetConfig) => void;
}

export default function ChartReorderCategories({
  label,
  mapDataSetConfig: initialMapDataSetConfig,
  onReorder,
}: Props) {
  const [categoricalDataConfig, setCategoricalDataConfig] = useState<
    MapCategoricalData[]
  >(initialMapDataSetConfig.categoricalDataConfig ?? []);

  const handleConfirm = () => {
    const updated = { ...initialMapDataSetConfig, categoricalDataConfig };
    onReorder(updated);
  };

  return (
    <ModalConfirm
      title="Reorder categories"
      triggerButton={
        <ButtonText>
          Reorder categories <VisuallyHidden> for {label}</VisuallyHidden>
        </ButtonText>
      }
      onConfirm={handleConfirm}
    >
      {categoricalDataConfig?.length > 0 ? (
        <>
          <p>Set the order of data categories in the map key.</p>
          <ReorderableList
            id="reorder-categories"
            list={categoricalDataConfig.map((category, index) => {
              return {
                id: `category-${index}`,
                label: category.value,
              };
            })}
            onMoveItem={({ prevIndex, nextIndex }) => {
              const reordered = reorder(
                categoricalDataConfig,
                prevIndex,
                nextIndex,
              );
              setCategoricalDataConfig(reordered);
            }}
          />
        </>
      ) : (
        <p>No categories available.</p>
      )}
    </ModalConfirm>
  );
}
