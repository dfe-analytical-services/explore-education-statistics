import * as React from 'react';
import {
  FormTextInput,
  FormFieldset,
  FormCheckbox,
  FormGroup,
} from '@common/components/form';
import { ChartDataSet } from '@common/services/publicationService';
import { DataBlockMetadata } from '@common/services/dataBlockService';

interface Props {
  id: string;
  title: string;
  dataSets: ChartDataSet[];
  meta: DataBlockMetadata;
}

const ChartAxisConfiguration = ({ id, title, dataSets, meta }: Props) => {
  const [units, setUnits] = React.useState(
    dataSets
      .map(dataSet => meta.indicators[dataSet.indicator])
      .filter(indicator => indicator !== null)
      .map(indicator => indicator.unit),
  );

  return (
    <FormFieldset id={id} legend={title}>
      <p>{title} configuration</p>
      <FormGroup>
        <FormCheckbox
          id={`${id}_show`}
          name={`${id}_show`}
          defaultChecked={false}
          label="Show axis?"
          value="show"
        />

        {units.length > 0 && <div>{units[0]}</div>}

        <FormTextInput
          id={`${id}_name`}
          name={`${id}_name`}
          defaultValue="hello"
          label="hello"
        />
      </FormGroup>
    </FormFieldset>
  );
};

export default ChartAxisConfiguration;
