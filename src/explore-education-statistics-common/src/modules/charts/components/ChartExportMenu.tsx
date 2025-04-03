import React, { useCallback } from 'react';
import downloadFile from '@common/utils/file/downloadFile';
import logger from '@common/services/logger';
import useCurrentPng from '@common/components/ChartsToPNG';
import ButtonText from '@common/components/ButtonText';
import ExportMenu from '@common/components/ExportMenu';

interface Props {
  chartTitle?: string | undefined;
}

export default function ChartExportMenu({ chartTitle }: Props) {
  const [getClipboardPng] = useCurrentPng();

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

  return (
    <ExportMenu title={chartTitle || 'chart'}>
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
    </ExportMenu>
  );
}
