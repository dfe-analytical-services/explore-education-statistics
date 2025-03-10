import React, { useState, useCallback, useEffect, useRef } from 'react';
import downloadFile from '@common/utils/file/downloadFile';
import { useCurrentPng, useGenerateImage } from './ChartsToPNG';
import ButtonText from './ButtonText';
import styles from './ExportButtonMenu.module.scss';

const ExportButtonMenu = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [getClipboardPng] = useCurrentPng();
  const dropdownRef = useRef<HTMLDivElement>(null);

  const [getDivJpeg] = useGenerateImage({
    quality: 0.8,
    type: 'image/jpeg',
  });

  const handleDivDownload = useCallback(async () => {
    const jpeg = await getDivJpeg();
    if (jpeg) {
      downloadFile(jpeg, 'Chart.png');
    }
  }, [getDivJpeg]);

  const handleCopyToClipboard = useCallback(async () => {
    await getClipboardPng(async blob => {
      try {
        if (blob) {
          const clipboardItems: ClipboardItem[] = [];
          const clipboardItem = new ClipboardItem({ [blob.type]: blob });
          clipboardItems.push(clipboardItem);
          await navigator.clipboard.write(clipboardItems);
        }
      } catch (e) {
        console.error(e);
      }
    });
  }, [getClipboardPng]);

  const handleCloseMenu = () => {
    setIsOpen(false);
  };

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  return (
    <div className={styles.exportDropdown}>
      <div ref={dropdownRef}>
        <ButtonText
          className={styles.exporButton}
          onClick={() => setIsOpen(!isOpen)}
          aria-label="Export options"
        >
          Export
        </ButtonText>
        {isOpen && (
          <div className={styles.exportMenu}>
            <ButtonText onClick={handleDivDownload}>
              Download chart as PNG image
            </ButtonText>
            <br />
            <ButtonText onClick={handleCopyToClipboard}>
              Copy chart to clipboard
            </ButtonText>
            <br />
            <ButtonText
              className={styles.exportCloseButton}
              onClick={handleCloseMenu}
            >
              Close
            </ButtonText>
          </div>
        )}
      </div>
    </div>
  );
};

export default ExportButtonMenu;
