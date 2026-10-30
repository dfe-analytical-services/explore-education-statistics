import downloadFile from '@common/utils/file/downloadFile';
import { useCallback } from 'react';

/**
 * Create a callback that downloads the PNG
 * from {@param getPng} as {@param fileName}.
 */
export default function usePngDownload(
  getPng: () => Promise<string | null>,
  fileName: string,
): () => Promise<void> {
  return useCallback(async () => {
    const png = await getPng();

    if (png) {
      downloadFile({
        file: png,
        extension: 'png',
        fileName,
      });
    }
  }, [fileName, getPng]);
}
