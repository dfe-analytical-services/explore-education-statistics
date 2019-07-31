import React, { useState } from 'react';
import { FormGroup, FormSelect } from '@common/components/form';
import PrototypeAdminEmbedTables from './PrototypeAdminEmbedTables';

// interface Props {
//   tableId?: string;
//   task?: string;
// }

const PrototypeExampleTable = () => {
  const [value, setValue] = useState('');

  const [addDatablock, setAddDataBlock] = useState(false);

  return (
    <React.Fragment>
      {!addDatablock && (
        <button
          type="button"
          className="govuk-button"
          onClick={() => setAddDataBlock(true)}
        >
          Add data block to this section
        </button>
      )}
      {addDatablock && (
        <>
          <FormGroup>
            <FormSelect
              id="select-table"
              name="select-table"
              label="Select a data block"
              value={value}
              onChange={event => {
                setValue(event.target.value);
              }}
              options={[
                {
                  label: 'Select a data block',
                  value: '',
                },
                {
                  label: 'Absence by characteristic table with chart',
                  value: 'table-1',
                },
                {
                  label: 'Absence highlights table only',
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
          {value === '' && (
            <button
              type="button"
              className="govuk-button govuk-button--secondary"
              onClick={() => {
                setValue('');
                setAddDataBlock(false);
              }}
            >
              Cancel
            </button>
          )}
        </>
      )}
      {value && value !== '' && (
        <>
          <PrototypeAdminEmbedTables />
        </>
      )}
      {addDatablock && value && value !== '' && (
        <React.Fragment>
          <button
            className="govuk-button govuk-!-margin-right-6"
            type="button"
            onClick={() => setAddDataBlock(false)}
          >
            Embed
          </button>
          <button
            type="button"
            className="govuk-button govuk-button--secondary"
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
