import getCsvData from '@common/modules/table-tool/components/utils/getCsvData';

// eslint-disable-next-line no-restricted-globals
const ctx: Worker = self as never;

/**
 * Process the csv data in a new thread and post the result back to the main thread.
 */
ctx.addEventListener('message', event => {
  const result = getCsvData(event.data.fullTable);
  ctx.postMessage({ csvData: result, fileName: event.data.fileName });
});

export {};
