import ButtonText from '@common/components/ButtonText';
import ExportMenu from '@common/components/ExportMenu';
import downloadFile from '@common/utils/file/downloadFile';
import React, { RefObject, useCallback, useEffect } from 'react';
import copyElementToClipboard from '@common/utils/copyElementToClipboard';
import useCurrentPng from '@common/components/ChartsToPNG';
import useToggle from '@common/hooks/useToggle';
import logger from '@common/services/logger';
import removeExportIgnoredElements from '@common/utils/dom/removeExportIgnoredElements';
import { Options as HTML2CanvasOptions } from 'html2canvas';

const pngOptions: Partial<HTML2CanvasOptions> = {
  onclone: (_, element) => removeExportIgnoredElements(element),
};

interface Props {
  containerRef: RefObject<HTMLElement | null>;
}

export default function KeyStatsExportMenu({ containerRef }: Props) {
  const [getClipboardPng] = useCurrentPng({
    options: pngOptions,
    ref: containerRef,
  });
  const [copySuccess, setCopySuccess] = useToggle(false);

  useEffect(() => {
    const resetTimeout = setTimeout(setCopySuccess.off, 5000);

    return () => {
      if (copySuccess) {
        clearTimeout(resetTimeout);
      }
    };
  }, [copySuccess, setCopySuccess]);

  const handlePngDownload = useCallback(async () => {
    const png = await getClipboardPng();
    if (png) {
      downloadFile({
        file: png,
        extension: 'png',
        fileName: 'Key Statistics Summary',
      });
    }
  }, [getClipboardPng]);

  const handleCopyToClipboard = useCallback(async () => {
    try {
      await copyElementToClipboard(containerRef);
      setCopySuccess.on();
    } catch (error) {
      logger.error(error);
    }
  }, [containerRef, setCopySuccess]);

  return (
    <ExportMenu title="headlines">
      <li role="menuitem">
        <ButtonText onClick={handleCopyToClipboard}>
          {copySuccess ? 'Copied' : 'Copy headlines to clipboard'}
        </ButtonText>
      </li>
      <li role="menuitem">
        <ButtonText onClick={handlePngDownload}>
          Download headlines as PNG
        </ButtonText>
      </li>
    </ExportMenu>
  );
}
