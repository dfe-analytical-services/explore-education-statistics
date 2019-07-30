import React, { useState } from 'react';
import { FormGroup, FormSelect } from '@common/components/form';
import PrototypeAdminExampleTables from './PrototypeAdminExampleTables';

// interface Props {
//   tableId?: string;
//   task?: string;
// }

const PrototypeExampleTable = () => {
  const [value, setValue] = useState('');

  const [addDatablock, setAddDataBlock] = useState(false);

  return (
    <React.Fragment>
      <button type="button" onClick={() => setAddDataBlock(true)}>
        Add data blocks to this section
      </button>

      {addDatablock && (
        <FormGroup>
          <FormSelect
            id="select-table"
            name="select-table"
            label="Select a table"
            value={value}
            onChange={event => {
              setValue(event.target.value);
            }}
            options={[
              {
                label: 'Select...',
                value: '',
              },
              {
                label: 'An example table for absence highlights panel',
                value: 'table-1',
              },
              {
                label: 'Example table 2',
                value: 'table-2',
              },
              {
                label: 'Example table 3',
                value: 'table-3',
              },
              {
                label: 'Example table 4',
                value: 'table-4',
              },
            ]}
          />
        </FormGroup>
      )}

      {value && value !== '' && <PrototypeAdminExampleTables />}

      {addDatablock && value && value !== '' && (
        <React.Fragment>
          <button type="button" onClick={() => setAddDataBlock(false)}>
            Embed
          </button>
          <button
            type="button"
            onClick={() => {
              setValue('');
              setAddDataBlock(false);
            }}
          >
            Cancel
          </button>
        </React.Fragment>
      )}
    </React.Fragment>
  );
};

export default PrototypeExampleTable;
