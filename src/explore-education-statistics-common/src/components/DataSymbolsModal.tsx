import Modal from '@common/components/Modal';
import React from 'react';
import DataSymbolsTable from '@common/modules/table-tool/components/DataSymbolsTable';
import InfoIcon from './InfoIcon';
import ButtonText from './ButtonText';

export default function DataSymbolsModal() {
  return (
    <Modal
      showClose
      title="Data symbols"
      triggerButton={
        <ButtonText>
          Data symbols <InfoIcon description="Data symbols guide" />
        </ButtonText>
      }
    >
      <DataSymbolsTable />
    </Modal>
  );
}
