import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import useToggle from '@common/hooks/useToggle';
import React, { RefObject } from 'react';
import { Footnote } from '@common/services/types/footnotes';
import downloadTableOdsFile from '@common/modules/table-tool/components/utils/downloadTableOdsFile';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import styles from './ExportTableButton.module.scss';

interface ExportTableButtonProps {
    fileName?: string | undefined,
    footnotes?: Footnote[];
    fullTable?: FullTable,
    tableRef: RefObject<HTMLElement>,
    title?: string | undefined,
}

const ExportTableButton: React.FC<ExportTableButtonProps> = ({
    fileName,
    footnotes = [],
    fullTable,
    title,
    tableRef,
 }) => {
    const [isOpen, toggleOpened] = useToggle(false);

    const tableFootnotes = fullTable
    ? fullTable.subjectMeta.footnotes
    : footnotes;

    const copyTableToClipboard = () => {
        if (!tableRef.current) return;
        
        const clipboardItem = new ClipboardItem({
          "text/plain": new Blob(
            [tableRef.current.innerText],
            { type: "text/plain" }
          ),
          "text/html": new Blob(
            [tableRef.current.outerHTML],
            { type: "text/html" }
          ),
        });
    
        navigator.clipboard.write([clipboardItem]);
      }

    const exportToOds = () => {
        downloadTableOdsFile(fileName ?? "table", tableFootnotes, tableRef, title ?? "table")
    };

    return (
        <Details
            summary="Export options"
            hiddenText="Export options for table"
            open={isOpen}
            onToggle={toggleOpened}
            className={styles.exportMenu}
        >
        {isOpen && (
            <ul
                className={`${styles.exportMenuList} govuk-!-font-size-16`}
                role="menu"
            >
                <li role="menuitem">
                    <ButtonText onClick={exportToOds}>
                        Download table as ODS
                    </ButtonText>
                </li>
                <li role="menuitem">
                    <ButtonText onClick={copyTableToClipboard}>
                        Copy table to clipboard
                    </ButtonText>
                </li>
                {/* need to add CSV functionality here but seems we need access to release table query */}
            </ul>
        )}
    </Details>
    );
};

export default ExportTableButton;