import React, { useState } from 'react';
import Details from '@common/components/Details';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';
import PrototypeAdminExampleTables from './PrototypeAdminExampleTables';

interface Props {
  tableId?: string;
  task?: string;
}

const PrototypeExampleTable = ({ tableId, task }: Props) => {
  const [value, setValue] = useState('academic-year');
  return (
    <>
      <Details summary="Add a table to this section">
        <FormGroup>
          <FormRadioGroup
            legend="Select a table"
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
        <a className="govuk-button" href="#">
          Add selected table option to section
        </a>
      </Details>
    </>
  );
};

export default PrototypeExampleTable;
