import 'core-js/stable'; // required for IE 11 support
import 'regenerator-runtime/runtime'; // required for IE 11 support
import { getCsvData } from '@common/modules/table-tool/components/utils/downloadCsv';

/* eslint-disable no-restricted-globals */
const ctx: Worker = self as never;

/**
 * Process the csv data in a new thread and post the result back to the main thread.
 */
ctx.addEventListener('message', event => {
  const result = getCsvData(event.data.fullTable);
  ctx.postMessage({ csvData: result, fileName: event.data.fileName }); // need filenmae?
});
export {};
