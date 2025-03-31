import React, { useCallback, useEffect, useRef } from 'react';
import downloadFile from '@common/utils/file/downloadFile';
import logger from '@common/services/logger';
import useToggle from '@common/hooks/useToggle';
import Details from '@common/components/Details';
import useCurrentPng from '@common/components/ChartsToPNG';
import ButtonText from '../../components/ButtonText';
import styles from './ChartExportButton.module.scss';

interface Props {
  chartTitle?: string | undefined;
}

const ChartExportButton = ({ chartTitle }: Props) => {
  const [isOpen, toggleOpened] = useToggle(false);
  const [getClipboardPng] = useCurrentPng();
  const containerRef = useRef<HTMLDivElement>(null);

  const handlePngDownload = useCallback(async () => {
    const png = await getClipboardPng();
    if (png) {
      downloadFile(png, chartTitle || 'Chart');
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

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        toggleOpened.off();
      }
    };

    const handleClickOutside = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        toggleOpened.off();
      }
    };

    if (isOpen) {
      document.addEventListener('keydown', handleKeyDown);
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen, toggleOpened]);

  return (
    <div ref={containerRef}>
      <Details
        summary="Export options"
        hiddenText={`Export options for ${chartTitle || 'chart'}`}
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
              <ButtonText onClick={handlePngDownload}>
                Download chart as PNG
              </ButtonText>
            </li>
            <li role="menuitem">
              <ButtonText onClick={handleCopyToClipboard}>
                Copy chart to clipboard
              </ButtonText>
            </li>
          </ul>
        )}
      </Details>
    </div>
  );
};

export default ChartExportButton;
