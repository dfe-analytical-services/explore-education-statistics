import ButtonText from '@common/components/ButtonText';
import { Footnote } from '@common/services/types/footnotes';
import downloadTableOdsFile from '@common/modules/table-tool/components/utils/downloadTableOdsFile';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import ExportMenu from '@common/components/ExportMenu';
import downloadFile from '@common/utils/file/downloadFile';
import React, { RefObject } from 'react';

interface Props {
  fileName?: string | undefined;
  footnotes?: Footnote[];
  fullTable?: FullTable;
  tableRef: RefObject<HTMLElement>;
  title?: string | undefined;
  onCsvDownload: () => Blob | Promise<Blob>;
}

export default function TableExportMenu({
  fileName,
  footnotes = [],
  fullTable,
  title,
  tableRef,
  onCsvDownload,
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

  const handleOdsDownload = () => {
    downloadTableOdsFile(
      fileName ?? 'table',
      tableFootnotes,
      tableRef,
      title ?? 'table',
    );
  };

  const handleCsvDownload = async () => {
    const csv = await onCsvDownload();
    downloadFile(csv, fileName);
  };

  return (
    <ExportMenu title={title || 'table'}>
      <li role="menuitem">
        <ButtonText onClick={handleOdsDownload}>
          Download table as ODS
        </ButtonText>
      </li>
      <li role="menuitem">
        <ButtonText onClick={handleCsvDownload}>
          Download table as CSV
        </ButtonText>
      </li>
      <li role="menuitem">
        <ButtonText onClick={copyTableToClipboard}>
          Copy table to clipboard
        </ButtonText>
      </li>
    </ExportMenu>
  );
}
