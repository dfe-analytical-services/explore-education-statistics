import { utils, writeFile } from 'xlsx';

export default function writeCsvFile(
  csvData: string[][],
  fileName: string,
): void {
  const workBook = utils.book_new();
  workBook.Sheets.Sheet1 = utils.aoa_to_sheet(csvData);
  workBook.SheetNames[0] = 'Sheet1';

  writeFile(workBook, `${fileName}.csv`, {
    type: 'binary',
  });
}
