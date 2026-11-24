import ButtonText from '@common/components/ButtonText';
import ExportMenu from '@common/components/ExportMenu';
import React, { RefObject, useCallback } from 'react';
import copyElementToClipboard from '@common/utils/copyElementToClipboard';
import useCurrentPng from '@common/components/ChartsToPNG';
import useCopySuccess from '@common/hooks/useCopySuccess';
import usePngDownload from '@common/hooks/usePngDownload';
import logger from '@common/services/logger';
import { Options as HTML2CanvasOptions } from 'html2canvas';

const pngOptions: Partial<HTML2CanvasOptions> = {
  ignoreElements: element =>
    element.matches('details:not([open]) > :not(summary)'),
};

interface Props {
  containerRef: RefObject<HTMLElement | null>;
  publicationTitle: string;
  releaseTitle: string;
}

export default function KeyStatsExportMenu({
  containerRef,
  publicationTitle,
  releaseTitle,
}: Props) {
  const [getClipboardPng] = useCurrentPng({
    options: pngOptions,
    ref: containerRef,
  });
  const [copySuccess, showCopySuccess] = useCopySuccess();

  const handlePngDownload = usePngDownload(
    getClipboardPng,
    // Replace slashes (e.g. 'Academic year 2016/17') with hyphens
    // so they aren't stripped out when the file name is sanitised.
    `Headlines - ${publicationTitle} - ${releaseTitle}`.replaceAll('/', '-'),
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
