import writeCsvFile from '@common/modules/table-tool/components/utils/writeCsvFile';
import { FullTable } from '@common/modules/table-tool/types/fullTable';

export default async function downloadTableCsvFile(
  fileName: string,
  fullTable: FullTable,
) {
  return new Promise<void>(resolve => {
    const worker = new Worker(
      new URL(
        '@common/modules/table-tool/components/workers/downloadTableCsv.worker',
        import.meta.url,
      ),
    );

    worker.postMessage({
      fileName,
      fullTable,
    });

    worker.onmessage = (event: MessageEvent) => {
      writeCsvFile(event.data.csvData, event.data.fileName);
      worker.terminate();
      resolve();
    };
  });
}
