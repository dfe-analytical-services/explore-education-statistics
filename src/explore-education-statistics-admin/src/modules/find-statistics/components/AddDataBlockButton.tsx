import React, { useState } from 'react';
import Button from '@common/components/Button';
import DatablockSelectForm from './DatablockSelectForm';

interface AddContentButtonProps {
  onAddDataBlock: (datablockId: string) => void;
}

const AddDataBlockButton = ({ onAddDataBlock }: AddContentButtonProps) => {
  const [showForm, setShowForm] = useState(false);

  return showForm ? (
    <DatablockSelectForm
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
      Add DataBlock
    </Button>
  );
};

export default AddDataBlockButton;
