import Button from '@common/components/Button';
import React, { useState } from 'react';
import DataBlockSelectForm from './DataBlockSelectForm';

interface AddContentButtonProps {
  onAddDataBlock: (datablockId: string) => void;
}

const AddDataBlockButton = ({ onAddDataBlock }: AddContentButtonProps) => {
  const [showForm, setShowForm] = useState(false);

  return showForm ? (
    <DataBlockSelectForm
      onSelect={selectedDataBlockId => {
        onAddDataBlock(selectedDataBlockId);
        setShowForm(false);
      }}
      onCancel={() => {
        setShowForm(false);
      }}
    />
  ) : (
    <Button variant="secondary" onClick={() => setShowForm(true)}>
      Add data block
    </Button>
  );
};

export default AddDataBlockButton;
