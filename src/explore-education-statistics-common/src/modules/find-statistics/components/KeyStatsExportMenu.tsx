import ButtonText from '@common/components/ButtonText';
import ExportMenu from '@common/components/ExportMenu';
import React, { RefObject, useCallback } from 'react';
import copyElementToClipboard from '@common/utils/copyElementToClipboard';
import useCurrentPng from '@common/components/ChartsToPNG';
import useCopySuccess from '@common/hooks/useCopySuccess';
import usePngDownload from '@common/hooks/usePngDownload';
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
  const [copySuccess, showCopySuccess] = useCopySuccess();

  const handlePngDownload = usePngDownload(
    getClipboardPng,
    'Key Statistics Summary',
  );

  const handleCopyToClipboard = useCallback(async () => {
    try {
      await copyElementToClipboard(containerRef);
      showCopySuccess();
    } catch (error) {
      logger.error(error);
    }
  }, [containerRef, showCopySuccess]);

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
