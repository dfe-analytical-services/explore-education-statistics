import React from 'react';
import { FormFieldset, FormTextInput } from '@common/components/form';
import FormTextArea from '@common/components/form/FormTextArea';
import { TableDataQuery } from '@common/modules/full-table/services/tableBuilderService';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import { DataBlockRequest, GeographicLevel, TimeIdentifier } from '@common/services/dataBlockService';
import Button from '@common/components/Button';


interface Props {
  query: TableDataQuery,
  tableHeaders: TableHeadersFormValues
};

const DataBlockDetailsForm = ({
  query,
  tableHeaders
}: Props) => {

  const [dataBlockTitle, setDataBlockTitle] = React.useState<string>();
  const [dataBlockSource, setDataBlockSource] = React.useState<string>();
  const [dataBlockFootnotes, setDataBlockFootnotes] = React.useState<string>();
  const [dataBlockName, setDataBlockName] = React.useState<string>();

  const saveDataBlock = () => {

    const dataBlockRequest: DataBlockRequest = {
      ...query,
      geographicLevel: query.geographicLevel as GeographicLevel,
      timePeriod: query.timePeriod && {
        ...query.timePeriod,
        startCode: query.timePeriod.startCode as TimeIdentifier,
        endCode: query.timePeriod.endCode as TimeIdentifier,
      },
    };


  };


  return (
    <div>
      <FormFieldset id="details" legend="Data block details">

        <FormTextInput
          id="data-block-title"
          name="data-block-title"
          label="Data block title"
          value={dataBlockTitle}
          onChange={e => setDataBlockTitle(e.target.value)}
        />

        <FormTextInput
          id="data-block-source"
          name="data-block-source"
          label="Source"
          width={10}
          value={dataBlockSource}
          onChange={e => setDataBlockSource(e.target.value)}
        />

        <FormTextArea
          id="data-block-footnotes"
          name="data-block-footnotes"
          label="Release footnotes"
          value={dataBlockFootnotes}
          onChange={e => setDataBlockFootnotes(e.target.value)}
        />

        <p>
          Name and save your datablock before viewing it under the 'View data blocks' tab at the top of this
          page.
        </p>

        <FormTextInput
          id="data-block-name"
          name="data-block-name"
          label="Name data block"
          value={dataBlockName}
          onChange={e => setDataBlockName(e.target.value)}
        />

        <Button
          type="button"
          onClick={() => {
            saveDataBlock();
          }}
        >
          Save
        </Button>
      </FormFieldset>
    </div>
  );
};

export default DataBlockDetailsForm;