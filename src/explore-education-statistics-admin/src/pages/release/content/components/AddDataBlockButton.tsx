import DataBlockSelectForm from '@admin/pages/release/content/components/DataBlockSelectForm';
import Button from '@common/components/Button';
import React, { useState } from 'react';

interface AddContentButtonProps {
  releaseId: string;
  onAddDataBlock: (datablockId: string) => void;
}

const AddDataBlockButton = ({
  releaseId,
  onAddDataBlock,
}: AddContentButtonProps) => {
  const [showForm, setShowForm] = useState(false);

  return showForm ? (
    <DataBlockSelectForm
      releaseId={releaseId}
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
