import React, { useCallback, useEffect } from 'react';
import downloadFile from '@common/utils/file/downloadFile';
import logger from '@common/services/logger';
import useToggle from '@common/hooks/useToggle';
import Details from '@common/components/Details';
import { useCurrentPng } from '../../components/ChartsToPNG';
import ButtonText from '../../components/ButtonText';
import styles from './ChartExportButton.module.scss';

interface Props {
  chartTitle: string | undefined;
}

const ChartExportButton = ({ chartTitle }: Props) => {
  const [isOpen, toggleOpened] = useToggle(false);
  const [getClipboardPng] = useCurrentPng();

  const handleDivDownload = useCallback(async () => {
    const jpeg = await getClipboardPng();
    if (jpeg) {
      downloadFile(jpeg, chartTitle || 'Chart');
    }
  }, [chartTitle, getClipboardPng]);

  const handleCopyToClipboard = useCallback(async () => {
    await getClipboardPng(async blob => {
      try {
        if (blob) {
          const clipboardItems: ClipboardItem[] = [];
          const clipboardItem = new ClipboardItem({ [blob.type]: blob });
          clipboardItems.push(clipboardItem);
          await navigator.clipboard.write(clipboardItems);
        }
      } catch (error) {
        logger.error(error);
      }
    });
  }, [getClipboardPng]);

  const handleCloseMenu = () => {
    toggleOpened.off();
  };

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        toggleOpened.off();
      }
    };

    if (isOpen) {
      document.addEventListener('keydown', handleKeyDown);
    }

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen, toggleOpened]);

  return (
    <Details
      summary="Export options"
      hiddenText={`Export options for ${chartTitle || 'chart'}`}
      open={isOpen}
      onToggle={toggleOpened}
      className={styles.exportMenu}
    >
      {isOpen && (
        <ul className={styles.exportMenuList} role="menu">
          <li role="menuitem">
            <ButtonText onClick={handleDivDownload}>
              Download chart as PNG
            </ButtonText>
          </li>
          <li role="menuitem">
            <ButtonText onClick={handleCopyToClipboard}>
              Copy chart to clipboard
            </ButtonText>
          </li>
          <li role="menuitem">
            <ButtonText
              className={styles.exportCloseButton}
              onClick={handleCloseMenu}
            >
              Close
            </ButtonText>
          </li>
        </ul>
      )}
    </Details>
  );
};

export default ChartExportButton;
