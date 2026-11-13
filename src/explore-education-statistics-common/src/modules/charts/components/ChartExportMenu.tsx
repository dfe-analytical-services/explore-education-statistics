import React, { RefObject, useCallback } from 'react';
import logger from '@common/services/logger';
import useCurrentPng from '@common/components/ChartsToPNG';
import ButtonText from '@common/components/ButtonText';
import ExportMenu from '@common/components/ExportMenu';
import useCopySuccess from '@common/hooks/useCopySuccess';
import usePngDownload from '@common/hooks/usePngDownload';

interface Props {
  chartRef: RefObject<HTMLElement | null>;
  chartTitle?: string | undefined;
}

export default function ChartExportMenu({ chartRef, chartTitle }: Props) {
  const [getClipboardPng] = useCurrentPng({ ref: chartRef });
  const [copySuccess, showCopySuccess] = useCopySuccess();

  const handlePngDownload = usePngDownload(
    getClipboardPng,
    chartTitle || 'Chart',
  );

  const handleCopyToClipboard = useCallback(async () => {
    try {
      await navigator.clipboard.write([
        // EES-6032 Safari requires the clipboard item value be returned from a promise
        // https://web.dev/articles/async-clipboard#write
        new ClipboardItem({
          // eslint-disable-next-line no-async-promise-executor
          'image/png': new Promise(async (resolve, reject) => {
            try {
              await getClipboardPng(blob => {
                if (!blob) {
                  throw new Error("Couldn't create blob");
                }
                resolve(blob);
              });
            } catch (err) {
              reject(err);
            }
          }),
        }),
      ]);

      showCopySuccess();
    } catch (error) {
      logger.error(error);
    }
  }, [getClipboardPng, showCopySuccess]);

  return (
    <ExportMenu title={chartTitle || 'chart'}>
      <li role="menuitem">
        <ButtonText onClick={handlePngDownload}>
          Download chart as PNG
        </ButtonText>
      </li>
      <li role="menuitem">
        <ButtonText onClick={handleCopyToClipboard}>
          {copySuccess ? 'Copied' : 'Copy chart to clipboard'}
        </ButtonText>
      </li>
    </ExportMenu>
  );
}
