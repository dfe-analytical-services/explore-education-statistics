import openPlaygroundUrl from '@common-test/openPlaygroundUrl';
import {
  TableCellJson,
  TableJson,
} from '@common/modules/table-tool/utils/mapTableToJson';
import { render } from '@testing-library/react';
import React from 'react';

/**
 * Debug a table by rendering it in the browser.
 */
export default async function debugTable({
  tbody,
  thead,
}: Partial<TableJson>): Promise<void> {
  const renderHeaders = (headers: TableCellJson[][]) =>
    headers.map((row, rowIndex) => (
      // eslint-disable-next-line react/no-array-index-key
      <tr key={rowIndex}>
        {row.map(({ tag: Tag, text, ...cell }, cellIndex) => (
          // eslint-disable-next-line react/no-array-index-key
          <Tag {...cell} key={cellIndex}>
            {text}

            {cell.colSpan !== undefined && (
              <div className="spanText">{`colSpan: ${cell.colSpan}`}</div>
            )}

            {cell.rowSpan !== undefined && (
              <div className="spanText">{`rowSpan: ${cell.rowSpan}`}</div>
            )}
          </Tag>
        ))}
      </tr>
    ));

  render(
    <>
      <style>
        {`
          table {
            padding: 40px;  
          }
          
          th,
          td {
            border: 1px solid #000;
            padding: 10px;
            font-family: monospace, sans;
            font-size: 18px;
          }
          
          [scope="colgroup"],
          [scope="rowgroup"] {
            background: #f5f5f5;
          }
          
          [scope="col"],
          [scope="row"] {
            background: #dedede;
          }
          
          .spanText {
            color: #444;
            font-size: 14px;
            font-weight: 300;
          }
        `}
      </style>

      <table>
        {thead && <thead>{renderHeaders(thead)}</thead>}
        {tbody && <tbody>{renderHeaders(tbody)}</tbody>}
      </table>
    </>,
  );

  await openPlaygroundUrl();
}
