import React, { useState } from 'react';
import Details from '@common/components/Details';
import {
  FormGroup,
  // FormFieldset,
  // FormTextInput,
  // FormSelect,
  FormRadioGroup,
} from '@common/components/form';
import PrototypeAdminExampleTables from './PrototypeAdminExampleTables';

// interface Props {
//   tableId?: string;
//   task?: string;
// }

const PrototypeExampleTable = () => {
  const [value, setValue] = useState('academic-year');
  return (
    <>
      <Details summary="Add data blocks to this section">
        <FormGroup>
          <FormRadioGroup
            legend="Select a data block"
            id="select-table"
            name="select-table"
            value={value}
            onChange={event => {
              setValue(event.target.value);
            }}
            options={[
              {
                id: 'table-1',
                label: 'An example table for absence highlights panel',
                value: 'table-1',
                conditional: <PrototypeAdminExampleTables task="selectTable" />,
              },
              {
                id: 'table-2',
                label: 'Example table 2',
                value: 'table-2',
                conditional: <PrototypeAdminExampleTables task="selectTable" />,
              },
              {
                id: 'table-3',
                label: 'Example table 3',
                value: 'table-3',
                conditional: <PrototypeAdminExampleTables task="selectTable" />,
              },
              {
                id: 'table-3',
                label: 'Example table 4',
                value: 'table-4',
                conditional: <PrototypeAdminExampleTables task="selectTable" />,
              },
              {
                id: 'no-table',
                label: 'Remove table',
                value: 'no-table',
              },
            ]}
          />
        </FormGroup>
      </Details>
    </>
  );
};

export default PrototypeExampleTable;
