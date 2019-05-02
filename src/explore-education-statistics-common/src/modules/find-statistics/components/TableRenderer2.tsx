import React from 'react';
import {
  DataBlockData,
  DataBlockMetadata,
} from '@common/services/dataBlockService';

export interface Props {
  data: DataBlockData;
  meta: DataBlockMetadata;
}

const TableRenderer2 = ({ data, meta }: Props) => {
  return (
    <table>
      <thead>
        <tr>
          <th>Heading</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td>Column</td>
        </tr>
      </tbody>
    </table>
  );
};

export default TableRenderer2;
