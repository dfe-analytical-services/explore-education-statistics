import ButtonText from '@common/components/ButtonText';
import { Footnote } from '@common/services/types/footnotes';
import downloadTableOdsFile from '@common/modules/table-tool/components/utils/downloadTableOdsFile';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import ExportMenu from '@common/components/ExportMenu';
import React, { RefObject } from 'react';

interface Props {
  fileName?: string | undefined;
  footnotes?: Footnote[];
  fullTable?: FullTable;
  tableRef: RefObject<HTMLElement>;
  title?: string | undefined;
}

export default function ExportTableButton({
  fileName,
  footnotes = [],
  fullTable,
  title,
  tableRef,
}: Props) {
  const tableFootnotes = fullTable
    ? fullTable.subjectMeta.footnotes
    : footnotes;

  const copyTableToClipboard = () => {
    if (!tableRef.current) return;

    const clipboardItem = new ClipboardItem({
      'text/plain': new Blob([tableRef.current.innerText], {
        type: 'text/plain',
      }),
      'text/html': new Blob([tableRef.current.outerHTML], {
        type: 'text/html',
      }),
    });

    navigator.clipboard.write([clipboardItem]);
  };

  const exportToOds = () => {
    downloadTableOdsFile(
      fileName ?? 'table',
      tableFootnotes,
      tableRef,
      title ?? 'table',
    );
  };

  return (
    <ExportMenu title={title || 'table'}>
      <li role="menuitem">
        <ButtonText onClick={exportToOds}>Download table as ODS</ButtonText>
      </li>
      <li role="menuitem">
        <ButtonText onClick={copyTableToClipboard}>
          Copy table to clipboard
        </ButtonText>
      </li>
    </ExportMenu>
  );
}
